using Vpassbackend.Data;
using Vpassbackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS for development - Allow any origin during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        // For Flutter development and troubleshooting, we're setting a very permissive CORS policy
        // WARNING: This is only for development! In production, restrict this to specific origins.
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition"); // For file downloads

        // NOTE: AllowAnyOrigin and AllowCredentials cannot be used together
        // If you need credentials, use specific origins instead of AllowAnyOrigin
    });

    // Add a more restrictive policy for production use
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:2027",  // Flutter web
                "http://127.0.0.1:2027",  // Flutter alternative
                "https://yourproductionsite.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFuelEfficiencyService, FuelEfficiencyService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DefaultIssuer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DefaultAudience",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultKeyMustBeLongEnoughForSecurity123456")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero // Strict token expiration validation
    };

    // For SPA applications
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["access_token"]
                ?? context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            return Task.CompletedTask;
        }
    };
});

// Add controller services & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Vehicle Passport API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await SeedData.SeedAsync(db);
        Console.WriteLine("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        // Continue with application startup even if seeding fails
    }
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Add a redirect from root to Swagger UI
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

// Important: CORS must come before other middleware
app.UseCors("AllowAnyOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Set up controllers with default routes
app.MapControllers();

// Configure routing options - unfortunately MapControllers doesn't take options directly
RouteOptions routeOptions = app.Services.GetRequiredService<IOptions<RouteOptions>>().Value;
routeOptions.LowercaseUrls = true; // This helps with case sensitivity

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next();

        // If a 404 occurs, it might be due to case sensitivity in routes
        if (context.Response.StatusCode == 404 && context.Request.Path.StartsWithSegments("/api"))
        {
            // Log the 404 for debugging
            Console.WriteLine($"404 Error: {context.Request.Method} {context.Request.Path}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Global error handler caught: {ex.Message}");
        await next();
    }
});

app.Run();