namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// The menus built for this app. Resolved from DI; the tree is created on first use so a
/// library can read its own <c>IOptions</c> while it adds links.
/// </summary>
public interface INetForgeMenuCatalog
{
    /// <summary>Every area that was mentioned while building. The default area is an empty string.</summary>
    IReadOnlyCollection<string> Areas { get; }

    /// <summary>The menu for <paramref name="area"/>. An unknown area is an empty menu.</summary>
    NetForgeMenuModel Get(string? area);
}
