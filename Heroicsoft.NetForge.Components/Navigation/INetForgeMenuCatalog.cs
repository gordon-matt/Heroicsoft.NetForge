namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// The menus built for this app. Resolved from DI; the tree is created on first use so a
/// library can read its own <c>IOptions</c> while it adds links.
/// </summary>
public interface INetForgeMenuCatalog
{
    /// <summary>Every menu name that was mentioned while building. The default menu is an empty string.</summary>
    IReadOnlyCollection<string> Names { get; }

    /// <summary>The menu for <paramref name="name"/>. An unknown name is an empty menu.</summary>
    NetForgeMenuModel Get(string? name);
}
