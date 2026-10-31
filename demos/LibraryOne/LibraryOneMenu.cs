using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace NetForge.Demo.LibraryOne;

/// <summary>Registers the first sample library on the host menu.</summary>
public sealed class LibraryOneMenuContributor : INetForgeMenuContributor
{
    public string Name => "LibraryOne";

    public string DefaultMenu => "Samples";

    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
    {
        group.AddCustom(item =>
        {
            item.Title = "Library 1";
            item.Icon = "book";
            item.Url = "/admin/library-one";
            configure?.Invoke(item);
        });
    }
}

/// <summary>Service and menu extensions for the first sample library.</summary>
public static class LibraryOneServices
{
    public static IServiceCollection AddLibraryOne(this IServiceCollection services) =>
        services.AddNetForgeMenuContributor<LibraryOneMenuContributor>();

    public static INetForgeMenuGroupBuilder AddLibraryOne(
        this INetForgeMenuGroupBuilder menu,
        Action<NetForgeMenuItemOptions>? configure = null) =>
        menu.AddContributor("LibraryOne", configure);
}
