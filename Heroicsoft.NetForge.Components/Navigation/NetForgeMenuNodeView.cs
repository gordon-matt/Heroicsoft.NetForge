using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Open state for one submenu. A side branch that contains the current page starts open.</summary>
public abstract class NetForgeMenuNodeView : ComponentBase, IDisposable
{
    private readonly object barId = new();
    private string? appliedPath;
    private bool open;
    private bool subscribed;
    private bool disposed;

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

    /// <summary>Shared by the top-level items of one menu, so only one dropdown is open.</summary>
    [CascadingParameter]
    protected NetForgeMenuBar? MenuBar { get; set; }

    /// <summary>
    /// Top-level items share <see cref="MenuBar"/>. A side branch, and a link already inside a dropdown, keep their own open state.
    /// </summary>
    protected bool UsesMenuBar => Top && !InDropdown && MenuBar is not null;

    /// <summary>
    /// Gets a value indicating whether this instance is open.
    /// </summary>
    protected bool Open => UsesMenuBar ? MenuBar!.IsOpen(barId) : open;

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
        if (!string.Equals(appliedPath, Path, StringComparison.Ordinal))
        {
            appliedPath = Path;

            if (UsesMenuBar)
            {
                // Top menus close on navigation. The bar is what the trigger reads, not the local flag.
                if (MenuBar!.IsOpen(barId))
                {
                    MenuBar.Close();
                }
            }
            else
            {
                // A side menu keeps the branch that contains the page open and closes the others.
                open = !Top && BranchCurrent;
            }
        }

        // Path can be applied on a render before the bar is cascaded, so subscription stays outside that check.
        if (UsesMenuBar && HasChildren && !subscribed)
        {
            MenuBar!.Changed += OnBarChanged;
            subscribed = true;
        }
    }

    /// <summary>
    /// Opens this dropdown, or closes it when it is already open.
    /// </summary>
    protected void Toggle()
    {
        if (UsesMenuBar)
        {
            MenuBar!.Toggle(barId);
            return;
        }

        open = !open;
    }

    /// <summary>
    /// Closes the value.
    /// </summary>
    protected void Close()
    {
        if (UsesMenuBar)
        {
            if (MenuBar!.IsOpen(barId))
            {
                MenuBar.Close();
            }

            return;
        }

        open = false;
    }

    /// <summary>
    /// Switches to this item when a dropdown is already open. A touch pointer does not count as a hover.
    /// </summary>
    /// <param name="args">The pointer event.</param>
    protected void OnPointerEnter(PointerEventArgs args)
    {
        if (!UsesMenuBar || !string.Equals(args.PointerType, "mouse", StringComparison.Ordinal))
        {
            return;
        }

        MenuBar!.Hover(HasChildren ? barId : null);
    }

    private void OnBarChanged()
    {
        if (!disposed)
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose()
    {
        disposed = true;

        if (!subscribed || MenuBar is null)
        {
            return;
        }

        MenuBar.Changed -= OnBarChanged;
        subscribed = false;

        if (MenuBar.IsOpen(barId))
        {
            MenuBar.Close();
        }
    }
}