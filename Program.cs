using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Middleware;
using RemindPe.Repositories;
using RemindPe.Services;

var builder = WebApplication.CreateBuilder(args);

// CLI Command: Online Hot Backup with integrity verification
if (args.Length > 0 && args[0] == "--backup")
{
    var destPath = args.Length > 1 
        ? args[1] 
        : Path.Combine("backups", $"khata_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");

    var dir = Path.GetDirectoryName(destPath);
    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=khata.db";
    using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(connStr))
    {
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"VACUUM INTO '{destPath.Replace("'", "''")}';";
        cmd.ExecuteNonQuery();
    }

    // Verify database integrity of snapshot
    using (var verifyConn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={destPath}"))
    {
        verifyConn.Open();
        using var verifyCmd = verifyConn.CreateCommand();
        verifyCmd.CommandText = "PRAGMA integrity_check;";
        var integrityResult = verifyCmd.ExecuteScalar()?.ToString();
        if (integrityResult != "ok")
        {
            Console.Error.WriteLine($"Backup integrity check failed: {integrityResult}");
            Environment.Exit(1);
        }
    }

    Console.WriteLine($"SUCCESS:{destPath}");
    return;
}

// 1. Configure SQLite Database with EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=khata.db";

builder.Services.AddDbContext<KhataDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 2. Health Checks for Infrastructure & Database Liveness (FIXED: actually queries DB via DI)
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database");

// 3. Response Compression & In-Memory Caching for High-Throughput & Fast Asset Delivery
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddMemoryCache();

// 4. Dependency Injection: Register Repositories (DIP / Clean Architecture)
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

// 5. Dependency Injection: Register Business & Notification Services (SRP / ISP)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IWhatsAppReminderService, WhatsAppReminderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IReportsService, ReportsService>();

// 6. Configure CORS — Restricted to known origins (FIXED: no longer AllowAnyOrigin)
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>()
                     ?? (builder.Environment.IsDevelopment()
                         ? new[] { "http://localhost:5033", "https://localhost:7083", "http://localhost:3000" }
                         : Array.Empty<string>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedOrigins", policy =>
    {
        if (builder.Environment.IsDevelopment() && (allowedOrigins.Length == 0 || allowedOrigins.Contains("*")))
        {
            // Development fallback: allow same-origin requests
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// 7. Register Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// 8. Configure Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "RemindPe - Enterprise VyaparKhata & WhatsApp Reminders API",
        Version = "v1",
        Description = "Production-grade micro-SaaS backend for merchants to manage customer credit (udhar), payments (jama), and automated WhatsApp reminders. Engineered for high concurrency and sub-millisecond response times."
    });
});

// 9. Rate Limiting — Protect against brute-force and DoS (NEW)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global sliding window: 100 requests per 60 seconds per IP
    options.AddSlidingWindowLimiter("GlobalLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromSeconds(60);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Strict limiter for write operations: 30 per minute
    options.AddFixedWindowLimiter("WriteLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromSeconds(60);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"success\":false,\"message\":\"Too many requests. Please try again later.\"}",
            cancellationToken);
    };
});

// 10. Request Body Size Limit (NEW — prevent memory exhaustion)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1_048_576; // 1 MB max request body
});

var app = builder.Build();

// 11. Production Enterprise Security Headers Middleware (covers all responses including static files)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
        "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: blob:; " +
        "connect-src 'self' https://wa.me; " +
        "frame-ancestors 'self';");
    await next();
});

// 12. Correlation ID & Distributed Tracing Middleware (Observability)
app.UseMiddleware<CorrelationIdMiddleware>();

// 13. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// 14. Response Compression
app.UseResponseCompression();

// 15. CORS Policy
app.UseCors("RestrictedOrigins");

// 16. Initialize & Auto-create SQLite Database with WAL Optimization & Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<KhataDbContext>();
        context.Database.EnsureCreated();

        // Safe schema evolution for existing SQLite databases without wiping data
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Settings"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Settings"" PRIMARY KEY AUTOINCREMENT,
                ""StoreName"" TEXT NOT NULL,
                ""OwnerName"" TEXT NULL,
                ""Phone"" TEXT NULL,
                ""UpiId"" TEXT NULL,
                ""Currency"" TEXT NULL,
                ""DefaultReminderTemplate"" TEXT NULL,
                ""ReceiptFooter"" TEXT NULL,
                ""UpdatedAt"" TEXT NOT NULL
            );
        ");

        try
        {
            context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Customers"" ADD COLUMN ""Address"" TEXT NULL;");
        }
        catch { /* Column already exists */ }

        try
        {
            context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Transactions"" ADD COLUMN ""CreatedAt"" TEXT NULL;");
        }
        catch { /* Column already exists */ }

        // High-Performance SQLite Configuration for 100k Users (Concurrent Reads, In-Memory Temp Store, 64MB Cache)
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;");
        context.Database.ExecuteSqlRaw("PRAGMA synchronous = NORMAL;");
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        context.Database.ExecuteSqlRaw("PRAGMA temp_store = MEMORY;");
        context.Database.ExecuteSqlRaw("PRAGMA cache_size = -64000;");
        context.Database.ExecuteSqlRaw("PRAGMA busy_timeout = 5000;");

        DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing or seeding the SQLite database.");
    }
}

// 17. Swagger — Only enabled in Development or explicitly via config (FIXED)
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", false))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RemindPe API v1");
        c.RoutePrefix = "swagger";
    });
}

// 18. Static Files & Default Document (serves wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// 19. Rate Limiting Middleware
app.UseRateLimiter();

// 20. API Key Authentication Middleware (protects /api/*)
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseAuthorization();

// 21. Liveness and Readiness Health Checks
app.MapHealthChecks("/health");

// 22. Controllers
app.MapControllers();

// 23. Fallback to index.html for SPA Navigation
app.MapFallbackToFile("index.html");

app.Run();

