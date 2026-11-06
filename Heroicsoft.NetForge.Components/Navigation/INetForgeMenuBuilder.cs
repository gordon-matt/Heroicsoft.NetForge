namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// Builds one menu's sections. Extension methods such as <c>AddLogVue</c> belong on
/// <see cref="INetForgeMenuGroupBuilder"/>, in the library that owns the links.
/// </summary>
public interface INetForgeMenuBuilder
{
    /// <summary>The application services. Available because the menu is built on first use, not at registration.</summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Draws this menu down the side. A group can still call <see cref="INetForgeMenuGroupBuilder.AsTopbar"/>.
    /// <paramref name="name"/> null means the menu currently being built.
    /// </summary>
    INetForgeMenuBuilder AsSidebar(string? name = null);

    /// <summary>
    /// Draws this menu across the top. A group can still call <see cref="INetForgeMenuGroupBuilder.AsSidebar"/>.
    /// <paramref name="name"/> null means the menu currently being built.
    /// </summary>
    INetForgeMenuBuilder AsTopbar(string? name = null);

    /// <summary>A labelled group of links in the menu currently being built.</summary>
    INetForgeMenuGroupBuilder AddMenu(string title, Action<NetForgeMenuGroupOptions>? configure = null);

    /// <summary>
    /// A labelled group filed under <paramref name="name"/>. The same title in the same menu appends
    /// links to the group that is already there.
    /// </summary>
    INetForgeMenuGroupBuilder AddMenu(string title, string name, Action<NetForgeMenuGroupOptions>? configure = null);

    /// <summary>
    /// Links with no heading, in the menu currently being built. A second call appends to that same section.
    /// </summary>
    INetForgeMenuGroupBuilder AddItems(Action<NetForgeMenuGroupOptions>? configure = null);

    /// <summary>Links with no heading, filed under <paramref name="name"/> instead of the menu currently being built.</summary>
    INetForgeMenuGroupBuilder AddItems(string name, Action<NetForgeMenuGroupOptions>? configure = null);

    /// <summary>
    /// Places every registered contributor that has not already been placed. A contributor's
    /// <see cref="INetForgeMenuContributor.DefaultMenu"/> and <see cref="INetForgeMenuContributor.DefaultName"/>
    /// choose the section. Call this after the explicit <c>AddContributor</c> lines.
    /// </summary>
    INetForgeMenuBuilder IncludeRegistered();
}

/// <summary>The target of <c>AddMenu</c>. Libraries extend this to add their own links.</summary>
public interface INetForgeMenuGroupBuilder
{
    /// <summary>
    /// Gets the services.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>A link the host describes itself. A library extension method is the same shape.</summary>
    INetForgeMenuGroupBuilder AddCustom(Action<NetForgeMenuItemOptions> configure);

    /// <summary>
    /// A nested menu. <paramref name="configure"/> runs after the children exist, so it can retitle
    /// the parent without wiping them.
    /// </summary>
    INetForgeMenuGroupBuilder AddSubmenu(
        string title,
        Action<INetForgeMenuGroupBuilder> children,
        Action<NetForgeMenuItemOptions>? configure = null);

    /// <summary>Places the contributor registered under <paramref name="name"/>.</summary>
    INetForgeMenuGroupBuilder AddContributor(string name, Action<NetForgeMenuItemOptions>? configure = null);

    /// <summary>Places every contributor that has not already been placed, into this group.</summary>
    INetForgeMenuGroupBuilder AddContributors();

    /// <summary>Draws this group in the sidebar. Other groups in the menu are unchanged.</summary>
    INetForgeMenuGroupBuilder AsSidebar();

    /// <summary>Draws this group in the top bar. Other groups stay where the menu put them.</summary>
    INetForgeMenuGroupBuilder AsTopbar();
}
