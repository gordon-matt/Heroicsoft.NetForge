namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Settings for one section inside a menu. The heading itself is the argument to <c>AddMenu</c>.</summary>
public sealed class NetForgeMenuGroupOptions
{
    /// <summary>Comma-separated roles. Empty shows the section to everyone.</summary>
    public string? Roles { get; set; }

    /// <summary>Lower numbers come first. Equal numbers keep registration order.</summary>
    public int Order { get; set; }

    /// <summary>
    /// Where this group is drawn. Null follows <c>AsTopbar</c> or <c>AsSidebar</c> on the area.
    /// </summary>
    public NetForgeMenuPlacement? Placement { get; set; }
}
