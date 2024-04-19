using NLog.Web;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

builder.Services.Configure<OtlpExporterOptions>("Logging", builder.Configuration.GetSection("OtlpExporterOptions"));

builder.Host.UseNLog();

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    var message = "testing";
    logger.LogInformation("message: {messageField}", message);
});

app.Run();
