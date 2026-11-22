using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Heroicsoft.NetForge.Tests;

public class NetForgeComponentRegistrationTests
{
    [Fact]
    public void AddNetForgeComponents_ProvidesASignInModeWhenTheHostHasNone()
    {
        var services = new ServiceCollection();
        services.AddNetForgeComponents();

        using var provider = services.BuildServiceProvider();

        Assert.Equal(AuthProvider.None, provider.GetRequiredService<IAuthProviderService>().Provider);
    }

    [Fact]
    public void AddNetForgeComponents_KeepsASignInModeTheHostAlreadyChose()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));
        services.AddNetForgeComponents();

        using var provider = services.BuildServiceProvider();

        Assert.Equal(AuthProvider.Identity, provider.GetRequiredService<IAuthProviderService>().Provider);
    }

    [Fact]
    public void AddNetForgeComponents_YieldsToASignInModeRegisteredAfterwards()
    {
        var services = new ServiceCollection();
        services.AddNetForgeComponents();
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Keycloak, "https://id.example"));

        using var provider = services.BuildServiceProvider();

        Assert.Equal(AuthProvider.Keycloak, provider.GetRequiredService<IAuthProviderService>().Provider);
    }

    [Fact]
    public void AddNetForgeToolbarItem_OrdersIconsAndKeepsRegistrationOrderWhenOrdersMatch()
    {
        var services = new ServiceCollection();
        services.AddNetForgeToolbarItem<Later>(20);
        services.AddNetForgeToolbarItem<FirstTied>();
        services.AddNetForgeToolbarItem<SecondTied>();

        using var provider = services.BuildServiceProvider();
        IReadOnlyList<ShellToolbarItem> ordered = ShellToolbarItems.InOrder(provider.GetServices<ShellToolbarItem>());

        Assert.Equal(
            [typeof(FirstTied), typeof(SecondTied), typeof(Later)],
            ordered.Select(item => item.ComponentType).ToArray());
    }

    private sealed class FirstTied : ComponentBase;

    private sealed class SecondTied : ComponentBase;

    private sealed class Later : ComponentBase;
}
