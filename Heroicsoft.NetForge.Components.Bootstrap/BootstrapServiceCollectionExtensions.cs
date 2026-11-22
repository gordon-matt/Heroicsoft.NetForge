using Heroicsoft.NetForge.Components.Bootstrap.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components.Bootstrap;

/// <summary>
/// Registers Bootstrap shell options on an <see cref="IServiceCollection"/>.
/// </summary>
public static class BootstrapServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Bootstrap component options and, through the shared package, the toast,
    /// dialog, and sidebar-refresh services. Use this instead of <c>AddNetForgeComponents</c> in
    /// apps built on Bootstrap and Bootswatch.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="configure">
    /// Sets <see cref="ForgeBootstrapOptions"/> at startup. <see langword="null"/> registers the options without extra configuration.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance, so calls can be chained.</returns>
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