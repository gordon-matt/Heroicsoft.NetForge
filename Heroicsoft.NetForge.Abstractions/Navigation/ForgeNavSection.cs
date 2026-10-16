namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>A labelled group of sidebar links. A null heading renders the items with no label.</summary>
public sealed class ForgeNavSection
{
    public string? Heading { get; init; }

    public string? Roles { get; init; }

    public IReadOnlyList<ForgeNavItem> Items { get; init; } = [];
}