namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>One entry in the shell sidebar.</summary>
public sealed class ForgeNavItem
{
    public required string Title { get; init; }

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

    /// <summary>Comma-separated roles. Empty means every signed-in user.</summary>
    public string? Roles { get; init; }
}