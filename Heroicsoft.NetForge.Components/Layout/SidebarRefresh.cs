namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Lets pages tell the sidebar its data is stale. The sidebar shows counts and lists that
/// change from pages outside its render tree. Scoped to the circuit.
/// </summary>
public sealed class SidebarRefresh
{
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}