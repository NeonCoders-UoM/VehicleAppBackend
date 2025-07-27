using Vpassbackend.Data;
using Vpassbackend.Services;
using Vpassbackend.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------- DATABASE ---------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------- CORS ---------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",         // React web
                "http://localhost:2027",         // Flutter mobile web dev
                "http://127.0.0.1:2027",
                "http://localhost:8081")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition"); // For file downloads
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",         // React web
                "http://localhost:2027",         // Flutter mobile web dev
                "http://127.0.0.1:2027",
                "http://localhost:8081",
                "https://yourproductionsite.com") // Add your production domain here
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --------------------- SERVICES ---------------------
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoyaltyPointsService, LoyaltyPointsService>();
builder.Services.AddScoped<IFuelEfficiencyService, FuelEfficiencyService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AppointmentPaymentService>();
builder.Services.AddScoped<ServiceCenterSearchService>();
builder.Services.AddScoped<DailyLimitService>();
builder.Services.AddScoped<AzureBlobService>();
builder.Services.AddHttpClient();

// --------------------- BACKGROUND SERVICES ---------------------
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddHostedService<ServiceReminderNotificationBackgroundService>();

// --------------------- JWT AUTHENTICATION ---------------------
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

// --------------------- CONTROLLERS & SWAGGER ---------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Passport API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --------------------- DATABASE SEED ---------------------
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
        Console.WriteLine($"Seeding error: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
    }
}

// --------------------- PIPELINE ---------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// HTTPS
app.UseHttpsRedirection();

// CORS - switch based on environment
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors");

// Handle preflight OPTIONS requests
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

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Set routing preferences
RouteOptions routeOptions = app.Services.GetRequiredService<IOptions<RouteOptions>>().Value;
routeOptions.LowercaseUrls = true;

// Global 404 and error handler
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
        Console.WriteLine($"Unhandled exception: {ex.Message}");
        await next();
    }
});

app.MapControllers();
app.Run();
