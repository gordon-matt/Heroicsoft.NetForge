using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Heroicsoft.NetForge.Components;

/// <summary>Registers the shared menu catalog and the callbacks that build menus.</summary>
public static class MenuServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared menu catalog. <see cref="BuildMenu(IServiceCollection, Action{INetForgeMenuBuilder})"/>
    /// calls this, and so does <c>AddNetForgeComponents</c>, so the component renders an empty menu
    /// until a host builds one.
    /// </summary>
    /// <param name="services">The collection that receives the catalog.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
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
    /// <param name="services">The collection that receives the registration.</param>
    /// <param name="configure">Adds groups and links. Runs on first use.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
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
    /// <param name="services">The collection that receives the registration.</param>
    /// <param name="name">Menu name. <see langword="null"/> or whitespace selects the unnamed menu.</param>
    /// <param name="configure">Adds groups and links. Runs on first use.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
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
    /// <typeparam name="TContributor">The contributor type. A second registration of the same type is ignored.</typeparam>
    /// <param name="services">The collection that receives the contributor.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddNetForgeMenuContributor<TContributor>(this IServiceCollection services)
        where TContributor : class, INetForgeMenuContributor
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddNetForgeMenu();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<INetForgeMenuContributor, TContributor>());
        return services;
    }

    /// <summary>Registers a library's links. A second instance of the same type is ignored.</summary>
    /// <param name="services">The collection that receives the contributor.</param>
    /// <param name="contributor">The library links to place later by name.</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="contributor"/> is <see langword="null"/>.</exception>
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