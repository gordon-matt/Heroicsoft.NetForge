namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>One entry in the shell sidebar.</summary>
public sealed class ForgeNavItem
{
    public required string Title { get; init; }

    public required string Href { get; init; }

    /// <summary>Bootstrap Icons class, without the <c>bi</c> prefix is also accepted by the sidebar.</summary>
    public string Icon { get; init; } = "bi-circle";

    public string? Badge { get; init; }

    public string BadgeCssClass { get; init; } = "text-bg-secondary";

    /// <summary>When true, the link is only active on an exact path match.</summary>
    public bool MatchAll { get; init; }

    /// <summary>Opens in a new tab. Use for dashboards that live outside the Blazor router, such as LogVue.</summary>
    public bool External { get; init; }

    /// <summary>Comma-separated roles. Empty means every signed-in user.</summary>
    public string? Roles { get; init; }
}