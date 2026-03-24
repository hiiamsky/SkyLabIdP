using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SkyLabIdP.WebApi.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING environment variable is not set.");

            var redisConnectionString = BuildRedisConnectionString();

            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: connectionString,
                    name: "sqlserver",
                    tags: ["ready"])
                .AddRedis(
                    redisConnectionString: redisConnectionString,
                    name: "redis",
                    tags: ["ready"]);

            return services;
        }

        public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
        {
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            }).AllowAnonymous();

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            }).AllowAnonymous();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();

            return app;
        }

        private static string BuildRedisConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("REDIS__HOST")
                ?? "localhost";
            var port = Environment.GetEnvironmentVariable("REDIS__PORT")
                ?? "6379";
            var password = Environment.GetEnvironmentVariable("REDIS__PASSWORD");

            var connectionString = $"{host}:{port},abortConnect=false";

            if (!string.IsNullOrWhiteSpace(password))
            {
                connectionString += $",password={password}";
            }

            return connectionString;
        }
    }
}
