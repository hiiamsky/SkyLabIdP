using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SkyLabIdP.WebApi.Extensions;

namespace Application.UnitTests.Infrastructure.WebApi;

public class OpenTelemetryExtensionsTests
{
    [Fact]
    public void AddOpenTelemetryObservability_RegistersTracerProvider()
    {
        var services = new ServiceCollection();

        services.AddOpenTelemetryObservability();

        var provider = services.BuildServiceProvider();
        var tracerProvider = provider.GetService<TracerProvider>();

        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenTelemetryObservability_RegistersMeterProvider()
    {
        var services = new ServiceCollection();

        services.AddOpenTelemetryObservability();

        var provider = services.BuildServiceProvider();
        var meterProvider = provider.GetService<MeterProvider>();

        meterProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenTelemetryObservability_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddOpenTelemetryObservability();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddOpenTelemetryObservability_IsIdempotent()
    {
        var services = new ServiceCollection();

        services.AddOpenTelemetryObservability();
        services.AddOpenTelemetryObservability();

        var provider = services.BuildServiceProvider();
        var tracerProvider = provider.GetService<TracerProvider>();

        tracerProvider.Should().NotBeNull();
    }
}
