using System.Security.Claims;
using System.Text.Json;

namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Copies Keycloak's nested role claims onto <see cref="ClaimTypes.Role"/> so
/// <c>IsInRole</c> works, and makes sure the stable user id is available as NameIdentifier.
/// </summary>
public static class KeycloakClaims
{
    public static void MapRoles(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var roleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var claim in identity.FindAll("realm_access").Concat(identity.FindAll("resource_access")))
        {
            try
            {
                using var document = JsonDocument.Parse(claim.Value);
                CollectRoles(document.RootElement, roleNames);
            }
            catch (JsonException)
            {
                // A non-JSON claim by that name is not something we can use; ignore it rather than
                // failing the sign-in.
            }
        }

        foreach (string role in roleNames)
        {
            if (identity.HasClaim(ClaimTypes.Role, role) || identity.HasClaim(identity.RoleClaimType, role))
            {
                continue;
            }

            identity.AddClaim(new Claim(identity.RoleClaimType, role));

            if (identity.RoleClaimType != ClaimTypes.Role)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }

    /// <summary>
    /// Keycloak's stable user id is <c>sub</c>. Copied onto NameIdentifier when inbound claim
    /// mapping did not, so a user's data is still theirs on the next sign-in.
    /// </summary>
    public static void EnsureNameIdentifier(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity
            || identity.HasClaim(claim => claim.Type == ClaimTypes.NameIdentifier))
        {
            return;
        }

        if (identity.FindFirst("sub")?.Value is { Length: > 0 } sub)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
        }
    }

    public static string? RewriteOrigin(string? url, string publicOrigin) => string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? url
            : Uri.TryCreate(publicOrigin, UriKind.Absolute, out var origin)
            ? new UriBuilder(uri) { Scheme = origin.Scheme, Host = origin.Host, Port = origin.Port }.Uri.ToString()
            : url;

    private static void CollectRoles(JsonElement element, HashSet<string> roleNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (element.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in roles.EnumerateArray())
            {
                if (role.GetString() is { Length: > 0 } name)
                {
                    roleNames.Add(name);
                }
            }
        }

        // resource_access is keyed by client id, so recurse one level to pick up per-client roles
        // as well as realm roles.
        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                CollectRoles(property.Value, roleNames);
            }
        }
    }
}