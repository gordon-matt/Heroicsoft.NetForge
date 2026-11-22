using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Heroicsoft.NetForge.Components;

/// <summary>Registers the NetForge component services on <see cref="IServiceCollection"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the toast, dialog, and sidebar-refresh services plus the options the nf-styled
    /// components read. Scoped to the circuit, so one user's toasts never appear on another's screen.
    /// The Bootstrap package calls this too and adds its own options on top.
    /// </summary>
    /// <param name="services">The collection that receives the registrations.</param>
    /// <param name="configure">Sets <see cref="ForgeUiOptions"/>. <see langword="null"/> registers the options with their defaults.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
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
    /// Draws <typeparamref name="TComponent"/> in the top bar, after the theme toggle and before the user menu.
    /// Each call adds another icon, so a library and the host can both contribute without replacing
    /// <see cref="ForgeShellOptions.TopbarEndComponent"/>. That component, when set, is drawn after these icons.
    /// </summary>
    /// <typeparam name="TComponent">The icon. It owns its button and anything the button opens.</typeparam>
    /// <param name="services">The collection that receives the registration.</param>
    /// <param name="order">Lower values sit closer to the theme toggle. Equal orders keep registration order. Defaults to 0.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection AddNetForgeToolbarItem<TComponent>(
        this IServiceCollection services,
        int order = 0)
        where TComponent : IComponent
    {
        services.AddSingleton(new ShellToolbarItem(typeof(TComponent), order));
        return services;
    }

    /// <summary>
    /// Turns on and configures toast sounds (egalink-toasty style). Sounds stay off unless this is
    /// called with <see cref="ForgeToastSoundOptions.EnableSounds"/> set to true.
    /// </summary>
    /// <param name="services">The collection that receives the configuration.</param>
    /// <param name="configure">Sets the sound options, including turning sounds on.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection ConfigureNetForgeToastSounds(
        this IServiceCollection services,
        Action<ForgeToastSoundOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
}