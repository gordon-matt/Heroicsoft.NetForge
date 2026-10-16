using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Components;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the toast, dialog, and sidebar-refresh services used by the shell components.
    /// Scoped to the circuit, so one user's toasts never appear on another's screen.
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
        services.AddScoped<Services.ToastService>();
        services.AddScoped<Services.DialogService>();
        services.AddScoped<Services.SidebarRefresh>();

        return services;
    }
}