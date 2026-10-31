using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace NetForge.Demo.LibraryTwo;

/// <summary>Registers the second sample library on the host menu.</summary>
public sealed class LibraryTwoMenuContributor : INetForgeMenuContributor
{
    public string Name => "LibraryTwo";

    public string DefaultMenu => "Samples";

    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
    {
        group.AddCustom(item =>
        {
            item.Title = "Library 2";
            item.Icon = "star";
            item.Url = "/admin/library-two";
            configure?.Invoke(item);
        });
    }
}

/// <summary>Service and menu extensions for the second sample library.</summary>
public static class LibraryTwoServices
{
    public static IServiceCollection AddLibraryTwo(this IServiceCollection services) =>
        services.AddNetForgeMenuContributor<LibraryTwoMenuContributor>();

    public static INetForgeMenuGroupBuilder AddLibraryTwo(
        this INetForgeMenuGroupBuilder menu,
        Action<NetForgeMenuItemOptions>? configure = null) =>
        menu.AddContributor("LibraryTwo", configure);
}
