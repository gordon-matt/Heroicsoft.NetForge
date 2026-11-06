namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Lets pages tell the sidebar its data is stale. The sidebar shows counts and lists that
/// change from pages outside its render tree. Scoped to the circuit.
/// </summary>
public sealed class SidebarRefresh
{
    /// <summary>
    /// Occurs when changed.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Notifies the changed.
    /// </summary>
    public void NotifyChanged() => Changed?.Invoke();
}