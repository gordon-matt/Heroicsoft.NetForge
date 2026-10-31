using System.Security.Claims;

namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>One link, or a submenu when <see cref="Children"/> is not empty.</summary>
public sealed class ForgeNavItem
{
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

    public string? Badge { get; init; }

    public string BadgeCssClass { get; init; } = "text-bg-secondary";

    /// <summary>Rendered after the title. Use this for a count that changes while the page is open. Static text uses <see cref="Badge"/>.</summary>
    public Type? BadgeComponent { get; init; }

    public IDictionary<string, object>? BadgeParameters { get; init; }

    /// <summary>When true, the link is only active on an exact path match.</summary>
    public bool MatchAll { get; init; }

    /// <summary>
    /// Paths that select this item, besides the path of <see cref="Href"/>.
    /// A list page that answers at both <c>/</c> and <c>/links</c> sets both.
    /// </summary>
    public IReadOnlyList<string>? ActivePaths { get; init; }

    /// <summary>
    /// Query parameters that must match before the link is active. A null value means that
    /// parameter is absent. Leave this null to compare the path only.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? ActiveQuery { get; init; }

    /// <summary>Opens in a new tab. Use for dashboards that live outside the Blazor router, such as LogVue.</summary>
    public bool External { get; init; }

    /// <summary>Comma-separated roles. Empty means the item is shown to everyone, including anonymous visitors.</summary>
    public string? Roles { get; init; }

    /// <summary>Nested links. The shared menu renders them under this item.</summary>
    public IReadOnlyList<ForgeNavItem> Children { get; init; } = [];

    /// <summary>
    /// Extra gate checked by the shared menu, after <see cref="Roles"/>. Null adds no further restriction.
    /// The delegate runs while the menu is rendered, so it must not depend on a disposed scope.
    /// </summary>
    public Func<ClaimsPrincipal, bool>? IsVisible { get; init; }

    /// <summary>Added to the link element by the shared menu.</summary>
    public string? CssClass { get; init; }
}