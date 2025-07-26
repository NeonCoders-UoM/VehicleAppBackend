using Vpassbackend.Data;
using Vpassbackend.Services;
using Vpassbackend.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS policy name
const string CorsPolicy = "UnifiedPolicy";

// Configure EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // React
                "http://localhost:2027",  // Flutter dev server
                "http://127.0.0.1:2027",  // Flutter alt
                "http://localhost:8081"   // Another Flutter web port
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoyaltyPointsService, LoyaltyPointsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFuelEfficiencyService, FuelEfficiencyService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<AzureBlobService>();
builder.Services.AddHttpClient();

// Background services
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddHostedService<ServiceReminderNotificationBackgroundService>();

builder.Services.AddScoped<AppointmentService>(provider =>
    new AppointmentService(
        provider.GetRequiredService<ApplicationDbContext>(),
        provider.GetRequiredService<INotificationService>(),
        provider.GetRequiredService<ILoyaltyPointsService>(),
        provider.GetRequiredService<ILogger<AppointmentService>>()
    )
);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DefaultIssuer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DefaultAudience",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultKeyMustBeLongEnough123456789")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

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

// Swagger
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
    }
}

// Development config
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

// CORS must come early
app.UseCors(CorsPolicy);

// Handle preflight
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Log 404s for /api
app.Use(async (context, next) =>
{
    try
    {
        await next();
        if (context.Response.StatusCode == 404 && context.Request.Path.StartsWithSegments("/api"))
        {
            Console.WriteLine($"404 Not Found: {context.Request.Method} {context.Request.Path}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Global error: {ex.Message}");
        await next();
    }
});

app.MapControllers();
app.Run();
