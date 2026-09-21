using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Formatting.Compact;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddObservabilityServices(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration.GetValue<string>("ServiceName")
                ?? throw new InvalidOperationException("ServiceName not found.");
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext =>
                    {
                        var path = httpContext.Request.Path;

                        return !path.StartsWithSegments("/health") &&
                               !path.StartsWithSegments("/metrics");
                    };
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(options =>
                {
                    var otlpExporterEndpoint = builder.Configuration.GetValue<string>("Otlp:Endpoint")
                        ?? throw new InvalidOperationException("Otlp:Endpoint not found.");
                    options.Endpoint = new Uri(otlpExporterEndpoint);

                    options.Protocol = OtlpExportProtocol.HttpProtobuf;

                    options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1000;

                    options.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 5000;
                }));

        builder.Host.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .WriteTo.Console(new CompactJsonFormatter()));

        return builder.Services;
    }

    public static void MapPrometheusScrapingEndpoint(WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
    }
}
