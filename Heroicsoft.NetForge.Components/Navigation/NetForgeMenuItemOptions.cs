using System.Security.Claims;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// One link while a menu is being built. Library defaults are assigned first; the host callback
/// then overwrites whichever properties it sets.
/// </summary>
public sealed class NetForgeMenuItemOptions
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Root-relative (<c>/logs</c>) or absolute. Leave empty when this item only groups <see cref="Children"/>.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// A NetForge icon name (<c>logs</c>), a Bootstrap Icons name (<c>bi-house</c>), or a font
    /// class list such as <c>fa fa-heart</c>. Class lists are emitted as an <c>i</c> element.
    /// </summary>
    public string Icon { get; set; } = "circle";

    public string? Tooltip { get; set; }

    /// <summary>Comma-separated roles. Empty shows the item to everyone.</summary>
    public string? Roles { get; set; }

    public bool External { get; set; }

    /// <summary>When true, the link is only active on an exact path match. Set this for <c>/</c>.</summary>
    public bool MatchAll { get; set; }

    public bool TagIcon { get; set; }

    public int? Count { get; set; }

    public string? Badge { get; set; }

    public string BadgeCssClass { get; set; } = "text-bg-secondary";

    public IReadOnlyList<string>? ActivePaths { get; set; }

    public IReadOnlyDictionary<string, string?>? ActiveQuery { get; set; }

    /// <summary>Lower numbers come first. Equal numbers keep registration order.</summary>
    public int Order { get; set; }

    public string? CssClass { get; set; }

    /// <summary>Checked in addition to <see cref="Roles"/> when the menu is rendered.</summary>
    public Func<ClaimsPrincipal, bool>? IsVisible { get; set; }

    public IList<NetForgeMenuItemOptions> Children => ChildItems;

    internal List<NetForgeMenuItemOptions> ChildItems { get; } = [];

    /// <summary>Adds a nested link and returns this item, so the parent can keep chaining.</summary>
    public NetForgeMenuItemOptions AddChild(Action<NetForgeMenuItemOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var child = new NetForgeMenuItemOptions();
        configure(child);
        ChildItems.Add(child);
        return this;
    }
}
