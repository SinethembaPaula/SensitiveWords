using System.Reflection;
using SensitiveWords.Api.Middleware;
using SensitiveWords.Application.Interfaces;
using SensitiveWords.Application.Services;
using SensitiveWords.Infrastructure.Caching;
using SensitiveWords.Infrastructure.Persistence.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/sensitivewords-api-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SensitiveWords API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    var connectionString = builder.Configuration.GetConnectionString("SensitiveWordsDb")
        ?? throw new InvalidOperationException("Connection string 'SensitiveWordsDb' is not configured.");

    builder.Services.AddControllers();
    builder.Services.AddMemoryCache();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "SensitiveWords API",
            Version = "v1",
            Description = "Sanitises messages by replacing sensitive words with asterisks."
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddScoped<ISensitiveWordRepository>(serviceProvider =>
    {
        var inner = new SensitiveWordRepository(
            connectionString,
            serviceProvider.GetRequiredService<ILogger<SensitiveWordRepository>>());

        var cache = serviceProvider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        var cacheLogger = serviceProvider.GetRequiredService<ILogger<CachedSensitiveWordRepository>>();

        return new CachedSensitiveWordRepository(inner, cache, cacheLogger);
    });

    builder.Services.AddScoped<IMessageSanitiserService, MessageSanitiserService>();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SensitiveWords API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
