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

// ============================================================================
// PDF PROCESSOR MODE: Uncomment the lines below to process PDFs, then run: dotnet run
// After processing, comment them out again to run the web API normally
// ============================================================================
// await Vpassbackend.Scripts.DirectPdfProcessor.Main(args);
// return;
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// --------------------- DATABASE ---------------------
// Priority: Environment Variable > appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not found. Set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --------------------- CORS ---------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlexibleCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin))
                return true; // Allow requests with no origin (mobile apps)

            var uri = new Uri(origin);

            // Allow specific React frontend
            if (origin == "http://localhost:3000")
                return true;

            // Allow any localhost with any port (for Flutter development)
            if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                return true;

            // Allow mobile app protocols
            if (origin.StartsWith("capacitor://") ||
                origin.StartsWith("ionic://") ||
                origin.StartsWith("file://"))
                return true;

            // Allow Vercel deployed Next.js frontend
            if (origin == "https://web-app-frontend-jtkoyn7az-kin-lgtms-projects.vercel.app")
                return true;

            // Allow any Vercel preview deployments
            if (origin.EndsWith(".vercel.app"))
                return true;

            // Add your production domains here
            if (origin == "https://yourproductionsite.com")
                return true;

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition");
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin))
                return true; // Allow requests with no origin (mobile apps)

            var uri = new Uri(origin);

            // Allow specific production domains
            if (origin == "https://yourproductionsite.com")
                return true;

            // Allow Vercel deployed Next.js frontend
            if (origin == "https://web-app-frontend-jtkoyn7az-kin-lgtms-projects.vercel.app")
                return true;

            // Allow any Vercel preview deployments
            if (origin.EndsWith(".vercel.app"))
                return true;

            // Allow localhost for development/testing (you can remove this in production)
            if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                return true;

            // Allow mobile app protocols
            if (origin.StartsWith("capacitor://") ||
                origin.StartsWith("ionic://") ||
                origin.StartsWith("file://"))
                return true;

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition");
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

// --------------------- GOOGLE MAPS SERVICE ---------------------
builder.Services.AddHttpClient<IGoogleMapsService, GoogleMapsService>();

// --------------------- CHATBOT RAG SERVICES ---------------------
builder.Services.AddHttpClient<OpenAIEmbeddingService>();
builder.Services.AddHttpClient<QdrantService>();
builder.Services.AddHttpClient<GroqService>();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<PdfKnowledgeService>();

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
            // Check for token in multiple places for maximum compatibility
            var token = context.Request.Cookies["access_token"] // Cookie (React)
                ?? context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last() // Bearer header
                ?? context.Request.Query["access_token"]; // Query parameter (mobile fallback)

            context.Token = token;
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

    // --------------------- AZURE BLOB STORAGE CONNECTION TEST ---------------------
    try
    {
        var blobService = scope.ServiceProvider.GetRequiredService<AzureBlobService>();
        // Test connection by attempting to list files (lightweight operation)
        await blobService.ListAllFilesAsync();
        Console.WriteLine("✓ Successfully connected to Azure Blob Storage");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Failed to connect to Azure Blob Storage: {ex.Message}");
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

// Enhanced CORS middleware with logging
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers.Origin.FirstOrDefault();

    // Log CORS requests for debugging
    if (!string.IsNullOrEmpty(origin))
    {
        Console.WriteLine($"CORS Request from origin: {origin}");
    }

    // Handle preflight OPTIONS requests
    if (context.Request.Method == "OPTIONS")
    {
        var corsPolicy = app.Environment.IsDevelopment() ? "FlexibleCors" : "ProductionCors";

        // Check if origin is allowed
        bool isAllowed = false;
        if (string.IsNullOrEmpty(origin))
        {
            isAllowed = true;
        }
        else
        {
            try
            {
                var uri = new Uri(origin);
                isAllowed = (origin == "http://localhost:3000") ||
                           (uri.Host == "localhost" || uri.Host == "127.0.0.1") ||
                           origin.StartsWith("capacitor://") ||
                           origin.StartsWith("ionic://") ||
                           origin.StartsWith("file://") ||
                           origin == "https://web-app-frontend-jtkoyn7az-kin-lgtms-projects.vercel.app" ||
                           origin.EndsWith(".vercel.app") ||
                           origin == "https://yourproductionsite.com";
            }
            catch
            {
                isAllowed = false;
            }
        }

        if (isAllowed)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin ?? "*";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
            context.Response.Headers["Access-Control-Allow-Headers"] =
                "Origin, X-Requested-With, Content-Type, Accept, Authorization, Cache-Control, Pragma";
            context.Response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition";
            context.Response.Headers["Access-Control-Max-Age"] = "86400"; // 24 hours
        }

        context.Response.StatusCode = 204;
        return;
    }

    await next();
});

// Apply CORS policy
app.UseCors(app.Environment.IsDevelopment() ? "FlexibleCors" : "ProductionCors");

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Set routing preferences
RouteOptions routeOptions = app.Services.GetRequiredService<IOptions<RouteOptions>>().Value;
routeOptions.LowercaseUrls = true;

// Global error handler with CORS headers
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

        // Ensure CORS headers are present in error responses
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal server error");
    }
});

app.MapControllers();

// Configure for actual device access
// Use 0.0.0.0 to listen on all network interfaces (accessible from real devices)
// Your device should connect to http://<YOUR_COMPUTER_IP>:5039
app.Run("http://0.0.0.0:5039");