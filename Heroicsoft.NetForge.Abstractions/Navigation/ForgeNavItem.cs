using System.Security.Claims;

namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>One link, or a submenu when <see cref="Children"/> is not empty.</summary>
public sealed class ForgeNavItem
{
    /// <summary>Text of the link or submenu.</summary>
    public required string Title { get; init; }

    /// <summary>Empty when this item only groups <see cref="Children"/> and is not itself a link.</summary>
    public required string Href { get; init; }

    /// <summary>Bootstrap Icons class, without the <c>bi</c> prefix is also accepted by the sidebar.</summary>
    public string Icon { get; init; } = "bi-circle";

    /// <summary>Hover text when it should say more than <see cref="Title"/>. The title is used when this is empty.</summary>
    public string? Tooltip { get; init; }

    /// <summary>Draws a small dot in place of <see cref="Icon"/>. For tag filters.</summary>
    public bool TagIcon { get; init; }

    /// <summary>Quiet trailing number. <see cref="Badge"/> is the pill; this is the plain count.</summary>
    public int? Count { get; init; }

    /// <summary>
    /// Static pill text shown after the title. A count that changes while the page is open uses <see cref="BadgeComponent"/>.
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>CSS class of the <see cref="Badge"/> pill. Defaults to <c>text-bg-secondary</c>.</summary>
    public string BadgeCssClass { get; init; } = "text-bg-secondary";

    /// <summary>Rendered after the title. Use this for a count that changes while the page is open. Static text uses <see cref="Badge"/>.</summary>
    public Type? BadgeComponent { get; init; }

    /// <summary>Arguments passed to <see cref="BadgeComponent"/> when it is rendered.</summary>
    public IDictionary<string, object>? BadgeParameters { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the link is only active on an exact path match.
    /// Otherwise a path also matches pages nested under it, and an empty path matches every page.
    /// </summary>
    public bool MatchAll { get; init; }

    /// <summary>
    /// Paths that select this item. A non-empty list replaces the path of <see cref="Href"/>
    /// instead of adding to it. A list page that answers at both <c>/</c> and <c>/links</c> sets both.
    /// </summary>
    public IReadOnlyList<string>? ActivePaths { get; init; }

    /// <summary>
    /// Query parameters that must match before the link is active. Names and values are compared
    /// without case. A <see langword="null"/> value means that parameter is absent or blank.
    /// An empty string requires the parameter to be present and empty.
    /// Leave this <see langword="null"/> to compare the path only.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? ActiveQuery { get; init; }

    /// <summary>Opens in a new tab. Use for a page outside this app's router, such as LogVue or Hangfire.</summary>
    public bool External { get; init; }

    /// <summary>Comma-separated roles. Empty means the item is shown to everyone, including anonymous visitors.</summary>
    public string? Roles { get; init; }

    /// <summary>Nested links. The shared menu renders them under this item.</summary>
    public IReadOnlyList<ForgeNavItem> Children { get; init; } = [];

    /// <summary>
    /// Extra gate checked by the shared menu, after <see cref="Roles"/>. <see langword="null"/> adds no further restriction.
    /// The delegate runs while the menu is rendered, so it must not depend on a disposed scope.
    /// </summary>
    public Func<ClaimsPrincipal, bool>? IsVisible { get; init; }

    /// <summary>Added to the link element by the shared menu.</summary>
    public string? CssClass { get; init; }
}