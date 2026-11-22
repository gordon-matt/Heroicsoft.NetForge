namespace Heroicsoft.NetForge.Authentication;

/// <summary>
/// Rewrites the host of Keycloak back-channel calls from the browser-facing authority to an
/// internally reachable base URL.
/// </summary>
/// <remarks>
/// In Docker the browser reaches Keycloak at something like <c>http://localhost:8081</c> while
/// the app container reaches the same server at <c>http://keycloak:8080</c>. The authority has
/// to stay the browser-facing value, because it appears in redirects and in the token's
/// <c>iss</c> claim, but discovery, token and userinfo requests are made by the app and must go
/// to the internal address. Without this, sign-in fails with a connection error that looks like
/// a Keycloak outage.
/// </remarks>
public sealed class KeycloakBackchannelHandler : HttpClientHandler
{
    private readonly Uri authority;
    private readonly Uri backchannelBase;

    /// <summary>Stores the browser-facing authority and the internal base URL used to rewrite back-channel calls.</summary>
    /// <param name="authority">Absolute browser-facing authority. Requests to this host are rewritten.</param>
    /// <param name="backchannelBaseUrl">Absolute base URL whose scheme, host, and port replace that host.</param>
    /// <exception cref="ArgumentNullException"><paramref name="authority"/> or <paramref name="backchannelBaseUrl"/> is <see langword="null"/>.</exception>
    /// <exception cref="UriFormatException">Either value is empty, relative, or not a valid absolute URI.</exception>
    public KeycloakBackchannelHandler(string authority, string backchannelBaseUrl)
    {
        this.authority = new Uri(authority, UriKind.Absolute);
        backchannelBase = new Uri(backchannelBaseUrl, UriKind.Absolute);
    }

    /// <summary>
    /// Sends <paramref name="request"/>, rewriting its scheme, host, and port to the back-channel
    /// base when the request host matches the authority host, ignoring case. The path and query
    /// stay. A request for any other host is sent unchanged.
    /// </summary>
    /// <param name="request">Outbound Keycloak request.</param>
    /// <param name="cancellationToken">Cancels the send.</param>
    /// <returns>The response from the rewritten URI, or from the original URI when the host did not match.</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null
            && string.Equals(request.RequestUri.Host, authority.Host, StringComparison.OrdinalIgnoreCase))
        {
            request.RequestUri = new UriBuilder(request.RequestUri)
            {
                Scheme = backchannelBase.Scheme,
                Host = backchannelBase.Host,
                Port = backchannelBase.Port,
            }.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}