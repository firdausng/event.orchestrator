using app.core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace app.core.Monitoring;

public static class MonitoringDependencyInjectionExtension
{
    public static IServiceCollection AddMonitoringService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PrometheusOptions>(configuration.GetSection(PrometheusOptions.SectionName));
        return services;
    }
    
    
}