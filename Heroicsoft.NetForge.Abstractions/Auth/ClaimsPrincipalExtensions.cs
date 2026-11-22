using System.Security.Claims;

namespace Heroicsoft.NetForge.Abstractions.Auth;

/// <summary>
/// Keycloak's user id lives on <c>sub</c>. ASP.NET maps that to
/// <see cref="ClaimTypes.NameIdentifier"/> when inbound mapping is on, and leaves it as
/// <c>sub</c> when it is not — so lookups have to accept both.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    private static readonly string[] UserIdClaimTypes =
    [
        ClaimTypes.NameIdentifier,
        "sub",
        "nameid",
    ];

    /// <summary>
    /// Returns the first non-whitespace user id on <paramref name="principal"/>.
    /// Looks at <see cref="ClaimTypes.NameIdentifier"/>, then <c>sub</c>, then <c>nameid</c>.
    /// Returns <see langword="null"/> when <paramref name="principal"/> is <see langword="null"/>
    /// or none of those claims has a value.
    /// </summary>
    /// <param name="principal">Principal to read. May be <see langword="null"/>.</param>
    /// <returns>The first matching claim value, or <see langword="null"/>.</returns>
    public static string? FindUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        foreach (string type in UserIdClaimTypes)
        {
            string? value = principal.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}