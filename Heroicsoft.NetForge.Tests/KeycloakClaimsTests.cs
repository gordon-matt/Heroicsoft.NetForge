using System.Security.Claims;
using System.Text.Json;
using Heroicsoft.NetForge.Authentication;

namespace Heroicsoft.NetForge.Tests;

public class KeycloakClaimsTests
{
    [Fact]
    public void MapRoles_FlattensRealmAndClientRoles()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("realm_access", JsonSerializer.Serialize(new { roles = new[] { "Administrator", "User" } })));
        identity.AddClaim(new Claim("resource_access", JsonSerializer.Serialize(new
        {
            client = new { roles = new[] { "User", "Editor" } },
        })));

        KeycloakClaims.MapRoles(new ClaimsPrincipal(identity));

        string[] roles = identity.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
        Assert.Contains("Administrator", roles);
        Assert.Contains("Editor", roles);
        Assert.Equal(3, roles.Length);
    }

    [Fact]
    public void MapRoles_IgnoresNonJsonClaims()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("realm_access", "not-json"));

        KeycloakClaims.MapRoles(new ClaimsPrincipal(identity));

        Assert.Empty(identity.FindAll(ClaimTypes.Role));
    }

    [Fact]
    public void EnsureNameIdentifier_CopiesSubWhenMissing()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("sub", "keycloak-sub"));

        KeycloakClaims.EnsureNameIdentifier(new ClaimsPrincipal(identity));

        Assert.Equal("keycloak-sub", identity.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void RewriteOrigin_ReplacesHostAndKeepsThePath()
    {
        string? rewritten = KeycloakClaims.RewriteOrigin(
            "http://internal:8080/signin-oidc",
            "https://app.example.com");

        Assert.NotNull(rewritten);
        var uri = new Uri(rewritten);
        Assert.Equal("https", uri.Scheme);
        Assert.Equal("app.example.com", uri.Host);
        Assert.Equal("/signin-oidc", uri.AbsolutePath);
    }
}