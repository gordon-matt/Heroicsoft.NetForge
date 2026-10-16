using Heroicsoft.NetForge.Components.Services;
using Heroicsoft.NetForge.Components.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the toast, dialog, and sidebar-refresh services plus the options the nf-styled
    /// components read. Scoped to the circuit, so one user's toasts never appear on another's screen.
    /// The Bootstrap package calls this too and adds its own options on top.
    /// </summary>
    public static IServiceCollection AddNetForgeComponents(
        this IServiceCollection services,
        Action<ForgeUiOptions>? configure = null)
    {
        if (configure is null)
        {
            services.AddOptions<ForgeUiOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        services.AddOptions<ForgeShellOptions>();
        services.AddScoped<ToastService>();
        services.AddScoped<DialogService>();
        services.AddScoped<SidebarRefresh>();

        return services;
    }
}
