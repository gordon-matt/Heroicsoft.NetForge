namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Lets pages tell the sidebar its data is stale. The sidebar shows counts and lists that
/// change from pages outside its render tree. Scoped to the circuit.
/// </summary>
public sealed class SidebarRefresh
{
    /// <summary>Raised by <see cref="NotifyChanged"/> so the shell layout reloads its sections.</summary>
    public event Action? Changed;

    /// <summary>Raises <see cref="Changed"/>. Pages call this after a write that the sidebar is showing.</summary>
    public void NotifyChanged() => Changed?.Invoke();
}