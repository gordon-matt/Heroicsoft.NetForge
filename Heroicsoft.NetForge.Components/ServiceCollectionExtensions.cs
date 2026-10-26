using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Heroicsoft.NetForge.Components.Toasts;
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
        services.AddOptions<ForgeToastSoundOptions>();
        services.AddScoped<ToastService>();
        services.AddScoped<DialogService>();
        services.AddScoped<SidebarRefresh>();

        return services;
    }

    /// <summary>
    /// Turns on and configures toast sounds (egalink-toasty style). Sounds stay off unless this is
    /// called with <see cref="ForgeToastSoundOptions.EnableSounds"/> set to true.
    /// </summary>
    public static IServiceCollection ConfigureNetForgeToastSounds(
        this IServiceCollection services,
        Action<ForgeToastSoundOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
}