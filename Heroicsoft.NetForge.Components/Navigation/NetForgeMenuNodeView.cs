using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Open state for one submenu. A side branch that contains the current page starts open.</summary>
public abstract class NetForgeMenuNodeView : ComponentBase
{
    private string? appliedPath;

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    [Parameter]
    public string Path { get; set; } = "/";

    /// <summary>The link sits in the top bar rather than a vertical list.</summary>
    [Parameter]
    public bool Top { get; set; }

    /// <summary>The link is already inside a dropdown, so children indent instead of opening a new bar.</summary>
    [Parameter]
    public bool InDropdown { get; set; }

    /// <summary>A section heading: no icon, children are the section's links.</summary>
    [Parameter]
    public bool Heading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is open.
    /// </summary>
    protected bool Open { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance has children.
    /// </summary>
    protected bool HasChildren => Item.Children is { Count: > 0 };

    /// <summary>
    /// Gets a value indicating whether this instance has href.
    /// </summary>
    protected bool HasHref => !string.IsNullOrWhiteSpace(Item.Href);

    /// <summary>
    /// Gets a value indicating whether this instance is current.
    /// </summary>
    protected bool Current => ForgeNavMatch.IsCurrent(Item, Path);

    /// <summary>
    /// Gets a value indicating whether this instance is branch current.
    /// </summary>
    protected bool BranchCurrent => ForgeNavMatch.IsInBranch(Item, Path);

    /// <summary>
    /// Gets the tip.
    /// </summary>
    protected string Tip => string.IsNullOrWhiteSpace(Item.Tooltip) ? Item.Title : Item.Tooltip;

    /// <summary>
    /// Gets the expand label.
    /// </summary>
    protected string ExpandLabel => "Expand " + Item.Title;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (string.Equals(appliedPath, Path, StringComparison.Ordinal))
        {
            return;
        }

        appliedPath = Path;

        // Top menus close on navigation. A side menu keeps the branch that contains the page open
        // and closes the others, which is the trail the reader is on.
        Open = !Top && BranchCurrent;
    }

    /// <summary>
    /// Toggles the value.
    /// </summary>
    protected void Toggle() => Open = !Open;

    /// <summary>
    /// Closes the value.
    /// </summary>
    protected void Close() => Open = false;
}