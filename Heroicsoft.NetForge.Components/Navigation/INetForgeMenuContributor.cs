namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// A library's links. Register it with <c>AddNetForgeMenuContributor</c> from the library's own
/// <c>Add</c> method. The host then places it with <c>AddContributor</c>, or calls
/// <c>IncludeRegistered</c> to drop every unplaced library into the group it names here.
/// </summary>
public interface INetForgeMenuContributor
{
    /// <summary>The name the host passes to <c>AddContributor</c>, compared without case.</summary>
    string Name { get; }

    /// <summary>Section heading used by <c>IncludeRegistered</c>. Defaults to <see cref="Name"/>.</summary>
    string DefaultMenu => Name;

    /// <summary>
    /// Area used by <c>IncludeRegistered</c>. Null uses the menu the host is building.
    /// </summary>
    string? DefaultArea => null;

    /// <summary>
    /// Adds this library's links to <paramref name="group"/>. Apply <paramref name="configure"/>
    /// to the primary item after the library defaults, so a host override of title or icon wins
    /// and an omitted override leaves the default in place.
    /// </summary>
    void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure);
}
