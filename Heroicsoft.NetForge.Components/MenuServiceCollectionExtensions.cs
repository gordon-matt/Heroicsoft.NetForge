using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Provides extension methods for menu service collections.
/// </summary>
public static class MenuServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared menu catalog. <see cref="BuildMenu(IServiceCollection, Action{INetForgeMenuBuilder})"/>
    /// calls this, and so does <c>AddNetForgeComponents</c>, so the component renders an empty menu
    /// until a host builds one.
    /// </summary>
    public static IServiceCollection AddNetForgeMenu(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<INetForgeMenuCatalog>(static services =>
            NetForgeMenuCatalog.Create(
                services,
                services.GetServices<NetForgeMenuRegistration>(),
                services.GetServices<INetForgeMenuContributor>()));

        return services;
    }

    /// <summary>Builds the unnamed menu. A later call for the same name appends sections.</summary>
    public static IServiceCollection BuildMenu(
        this IServiceCollection services,
        Action<INetForgeMenuBuilder> configure) =>
        services.BuildMenu(NetForgeMenus.DefaultName, configure);

    /// <summary>
    /// Builds the menu addressed by <paramref name="name"/>, the same string as
    /// <c>ForgeShellOptions.SharedMenuName</c> and <c>NetForgeMenu.Name</c>. The value selects
    /// the menu. It does not filter by URL. The callback runs on first use, so extensions can
    /// read <c>IOptions</c> and other singletons. Do not resolve a scoped service from it.
    /// </summary>
    public static IServiceCollection BuildMenu(
        this IServiceCollection services,
        string name,
        Action<INetForgeMenuBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddNetForgeMenu();
        services.AddSingleton(new NetForgeMenuRegistration(Normalize(name), configure));
        return services;
    }

    /// <summary>Registers a library's links so a host can place them by name.</summary>
    public static IServiceCollection AddNetForgeMenuContributor<TContributor>(this IServiceCollection services)
        where TContributor : class, INetForgeMenuContributor
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddNetForgeMenu();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<INetForgeMenuContributor, TContributor>());
        return services;
    }

    /// <summary>Registers a library's links. A second instance of the same type is ignored.</summary>
    public static IServiceCollection AddNetForgeMenuContributor(
        this IServiceCollection services,
        INetForgeMenuContributor contributor)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(contributor);

        services.AddNetForgeMenu();
        services.TryAddEnumerable(ServiceDescriptor.Singleton(contributor));
        return services;
    }

    private static string Normalize(string? name) =>
        string.IsNullOrWhiteSpace(name) ? NetForgeMenus.DefaultName : name.Trim();
}