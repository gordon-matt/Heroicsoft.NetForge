using Heroicsoft.NetForge.WebAssets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Heroicsoft.NetForge.Components;

/// <summary>Serves the NetForge stylesheets and scripts from the component assemblies.</summary>
public static class NetForgeAssetEndpoints
{
    /// <summary>
    /// Maps <c>/_content/Heroicsoft.NetForge.Components/</c> and, when that package is loaded,
    /// <c>/_content/Heroicsoft.NetForge.Components.Bootstrap/</c>.
    /// A host that already referenced the packages keeps the static web assets from that reference.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The same route builder.</returns>
    public static IEndpointRouteBuilder MapNetForgeAssets(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapEmbeddedContent(typeof(NetForgeAssetEndpoints).Assembly);

        var bootstrap = EmbeddedContent.Resolve(
            "Heroicsoft.NetForge.Components.Bootstrap",
            typeof(NetForgeAssetEndpoints).Assembly);
        if (bootstrap is not null)
        {
            endpoints.MapEmbeddedContent(bootstrap);
        }

        return endpoints;
    }
}
