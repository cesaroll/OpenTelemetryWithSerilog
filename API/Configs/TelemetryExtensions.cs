using Ardalis.GuardClauses;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace API.Configs
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddLoggingAndTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = GetOpenTelemetrySettings(configuration);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(x => {
                    x.Endpoint = settings.Endpoint;
                    x.Protocol = OtlpProtocol.HttpProtobuf;
                    x.Headers = new Dictionary<string, string>
                    {
                        ["X-Seq-ApiKey"] = settings.Secret
                    };
                    x.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = settings.AppName
                    };
                })
                .CreateLogger();

            services.AddSerilog();

            return services;
        }

        private static OpenTelemetrySettings GetOpenTelemetrySettings(IConfiguration configuration)
        {
            var settings = new OpenTelemetrySettings();
            configuration.GetSection("OpenTelemetry").Bind(settings);

            Guard.Against.NullOrWhiteSpace(settings.AppName, "AppName", "Application Name is Missing in OpenTelemetry Settings");
            Guard.Against.NullOrWhiteSpace(settings.Endpoint, "Endpoint", "Endpoint is Missing in OpenTelemetry Settings");
            Guard.Against.NullOrWhiteSpace(settings.Secret, "Secret", "Secret is Missing in OpenTelemetry Settings");

            return settings;
        }
    }
}
