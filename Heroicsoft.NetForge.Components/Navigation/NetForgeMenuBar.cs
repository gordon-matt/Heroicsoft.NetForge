namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// Which top-level dropdown is open. A click arms the bar. Hover can then switch menus, including
/// after the pointer passes a top-level item that has no dropdown. Clicking outside, pressing Escape,
/// or clicking the open item disarms it, and hover does nothing until the next click.
/// </summary>
public sealed class NetForgeMenuBar
{
    /// <summary>Raised when the open item or the armed state changes, so every top-level item can redraw.</summary>
    public event Action? Changed;

    /// <summary>The open top-level item, or <see langword="null"/> when no dropdown panel is showing.</summary>
    public object? OpenItem { get; private set; }

    /// <summary>
    /// True from the click that opens a menu until the bar is dismissed.
    /// Hovering a top-level item with no dropdown hides the panel without clearing this.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets a value indicating whether a dropdown panel is showing.</summary>
    public bool AnyOpen => OpenItem is not null;

    /// <summary>
    /// Whether <paramref name="item"/> is the open dropdown.
    /// </summary>
    /// <param name="item">The top-level item.</param>
    /// <returns>true when that item's dropdown is open.</returns>
    public bool IsOpen(object item) => ReferenceEquals(OpenItem, item);

    /// <summary>
    /// Opens <paramref name="item"/>, or closes it when it is already open.
    /// </summary>
    /// <param name="item">The top-level item that was clicked.</param>
    public void Toggle(object item)
    {
        if (ReferenceEquals(OpenItem, item))
        {
            OpenItem = null;
            IsActive = false;
        }
        else
        {
            OpenItem = item;
            IsActive = true;
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Switches the open dropdown to <paramref name="item"/>. A null item hides the panel but leaves the bar armed.
    /// Does nothing while the bar is idle, so hovering cannot open the first menu.
    /// </summary>
    /// <param name="item">The top-level item under the pointer, or null when that item has no dropdown.</param>
    public void Hover(object? item)
    {
        if (!IsActive || ReferenceEquals(OpenItem, item))
        {
            return;
        }

        OpenItem = item;
        Changed?.Invoke();
    }

    /// <summary>Closes the open dropdown and disarms the bar.</summary>
    public void Close()
    {
        if (!IsActive && OpenItem is null)
        {
            return;
        }

        OpenItem = null;
        IsActive = false;
        Changed?.Invoke();
    }
}
