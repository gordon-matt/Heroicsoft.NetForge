using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Provides extension methods for service collections.
/// </summary>
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
        services.AddNetForgeMenu();
        services.AddScoped<ToastService>();
        services.AddScoped<DialogService>();
        services.AddScoped<SidebarRefresh>();

        // The top bar and the sign-in redirect inject this. A host that never calls the authentication
        // package has no sign-in of its own, so the shell shows no sign-in menu. TryAdd keeps a mode
        // the host already chose, and a registration added afterwards still wins.
        services.TryAddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.None, null));

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