using System.Security.Claims;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// One link while a menu is being built. Library defaults are assigned first; the host callback
/// then overwrites whichever properties it sets.
/// </summary>
public sealed class NetForgeMenuItemOptions
{
    /// <summary>Text of the link. A blank title is rejected when the menu is built.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Root-relative (<c>/logs</c>) or absolute. Leave empty when this item only groups <see cref="Children"/>.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// A NetForge icon name (<c>logs</c>), a Bootstrap Icons name (<c>bi-house</c>), or a font
    /// class list such as <c>fa fa-heart</c>. Class lists are emitted as an <c>i</c> element.
    /// </summary>
    public string Icon { get; set; } = "circle";

    /// <summary>Hover text. <see langword="null"/> or whitespace uses <see cref="Title"/> when the link is rendered.</summary>
    public string? Tooltip { get; set; }

    /// <summary>Comma-separated roles. Empty shows the item to everyone.</summary>
    public string? Roles { get; set; }

    /// <summary>Opens the link in a new tab. <see langword="false"/>, the default, stays in this tab. An external link shows an external icon instead of a count or badge.</summary>
    public bool External { get; set; }

    /// <summary>When true, the link is only active on an exact path match. Set this for <c>/</c>.</summary>
    public bool MatchAll { get; set; }

    /// <summary>Draws a colored dot instead of <see cref="Icon"/>. <see langword="false"/> by default. Used for tag filters.</summary>
    public bool TagIcon { get; set; }

    /// <summary>Plain number after the label. <see langword="null"/> hides it. When set, it is shown instead of <see cref="Badge"/>.</summary>
    public int? Count { get; set; }

    /// <summary>Short pill after the label. <see langword="null"/> or empty hides it. Ignored when <see cref="Count"/> is set or the link is external.</summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Tone of <see cref="Badge"/>. Defaults to <c>text-bg-secondary</c>, which maps to the muted NetForge badge. An <c>nf-badge-*</c> class is used as written.
    /// </summary>
    public string BadgeCssClass { get; set; } = "text-bg-secondary";

    /// <summary>
    /// Paths that mark the link current. When the list has entries, they replace the path of <see cref="Url"/>. <see langword="null"/> or empty uses the path of <see cref="Url"/>.
    /// </summary>
    public IReadOnlyList<string>? ActivePaths { get; set; }

    /// <summary>
    /// Query keys that must match before the link is current. A <see langword="null"/> value means that key is absent. A <see langword="null"/> dictionary compares the path only.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? ActiveQuery { get; set; }

    /// <summary>Lower numbers come first. Equal numbers keep registration order.</summary>
    public int Order { get; set; }

    /// <summary>Extra classes on the link. <see langword="null"/> adds none.</summary>
    public string? CssClass { get; set; }

    /// <summary>Checked in addition to <see cref="Roles"/> when the menu is rendered.</summary>
    public Func<ClaimsPrincipal, bool>? IsVisible { get; set; }

    /// <summary>The nested links. <see cref="AddChild"/> appends to this list.</summary>
    public IList<NetForgeMenuItemOptions> Children => ChildItems;

    internal List<NetForgeMenuItemOptions> ChildItems { get; } = [];

    /// <summary>Adds a nested link and returns this item, so the parent can keep chaining.</summary>
    /// <param name="configure">Sets the child. Required.</param>
    /// <returns>This item, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public NetForgeMenuItemOptions AddChild(Action<NetForgeMenuItemOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var child = new NetForgeMenuItemOptions();
        configure(child);
        ChildItems.Add(child);
        return this;
    }
}