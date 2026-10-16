using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;

namespace Heroicsoft.NetForge.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void FindUserId_PrefersNameIdentifier()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "mapped-id"),
            new Claim("sub", "keycloak-sub"),
        ]));

        Assert.Equal("mapped-id", user.FindUserId());
    }

    [Fact]
    public void FindUserId_FallsBackToSub()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "keycloak-sub")]));

        Assert.Equal("keycloak-sub", user.FindUserId());
    }

    [Fact]
    public void FindUserId_ReturnsNullWhenMissing()
    {
        Assert.Null(new ClaimsPrincipal(new ClaimsIdentity()).FindUserId());
        Assert.Null(((ClaimsPrincipal?)null).FindUserId());
    }
}