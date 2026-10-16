using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components.Bootstrap;

public static class BootstrapServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Bootstrap component options and, through the shared package, the toast,
    /// dialog, and sidebar-refresh services. Use this instead of <c>AddNetForgeComponents</c> in
    /// apps built on Bootstrap and Bootswatch.
    /// </summary>
    public static IServiceCollection AddNetForgeBootstrapComponents(
        this IServiceCollection services,
        Action<ForgeBootstrapOptions>? configure = null)
    {
        if (configure is null)
        {
            services.AddOptions<ForgeBootstrapOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services.AddNetForgeComponents();
    }
}
