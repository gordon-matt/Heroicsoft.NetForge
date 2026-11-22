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
    /// The link or submenu this node draws. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavItem Item { get; set; } = default!;

    /// <summary>
    /// Current path and query. A change closes an open top-bar dropdown, and opens a side branch that contains the page. Defaults to <c>/</c>.
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

    /// <summary><see langword="true"/> while this node's children are showing.</summary>
    protected bool Open => UsesMenuBar ? MenuBar!.IsOpen(barId) : open;

    /// <summary><see langword="true"/> when the item has child links.</summary>
    protected bool HasChildren => Item.Children is { Count: > 0 };

    /// <summary><see langword="true"/> when the item has an address. A side branch with an address shows that link beside a separate expand button.</summary>
    protected bool HasHref => !string.IsNullOrWhiteSpace(Item.Href);

    /// <summary><see langword="true"/> when this link is the current page.</summary>
    protected bool Current => ForgeNavMatch.IsCurrent(Item, Path);

    /// <summary><see langword="true"/> when this link or a nested child is the current page.</summary>
    protected bool BranchCurrent => ForgeNavMatch.IsInBranch(Item, Path);

    /// <summary>Hover text. The item tooltip when it is set; otherwise the title.</summary>
    protected string Tip => string.IsNullOrWhiteSpace(Item.Tooltip) ? Item.Title : Item.Tooltip;

    /// <summary>Accessible name of the side-branch expand button: <c>Expand</c> plus the item title.</summary>
    protected string ExpandLabel => "Expand " + Item.Title;

    /// <summary>
    /// Opens a side branch that contains the current page and closes the others. Closes a top-bar dropdown that is open when the path changes.
    /// </summary>
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

    /// <summary>Hides this node's children. A top-bar item also disarms the shared bar when this item was the open one.</summary>
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

    /// <summary>Stops redrawing for the shared bar and closes this item's dropdown if it is the open one.</summary>
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