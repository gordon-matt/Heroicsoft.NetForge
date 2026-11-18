using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace Heroicsoft.NetForge.WebAssets;

/// <summary>
/// Serves files embedded from a library's <c>wwwroot</c> folder at <c>/_content/{AssemblyName}/</c>.
/// Import <c>Heroicsoft.NetForge.WebAssets.targets</c> so the folder is embedded with stable names.
/// A host that referenced the library keeps its static web assets: those endpoints are more specific
/// and are chosen first.
/// </summary>
public static class EmbeddedContent
{
    private const string Folder = ".wwwroot/";

    private static readonly ConcurrentDictionary<Assembly, IReadOnlyDictionary<string, Asset>> Catalogs = new();

    /// <summary>
    /// Maps <c>/_content/{assembly name}/{path}</c> to the embedded <c>wwwroot</c> of <paramref name="assembly"/>.
    /// Calling it again for the same assembly does nothing.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="assembly">The assembly whose embedded <c>wwwroot</c> should be served.</param>
    /// <returns>The same route builder.</returns>
    public static IEndpointRouteBuilder MapEmbeddedContent(this IEndpointRouteBuilder endpoints, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(assembly);

        string? name = assembly.GetName().Name;
        if (string.IsNullOrEmpty(name))
        {
            return endpoints;
        }

        string endpointName = "Heroicsoft.NetForge.WebAssets:" + name;
        if (endpoints.DataSources
            .SelectMany(source => source.Endpoints)
            .Any(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == endpointName))
        {
            return endpoints;
        }

        endpoints.MapGet($"/_content/{name}/{{**path}}", (string? path, HttpContext context) => Serve(assembly, path, context))
            .WithName(endpointName)
            .WithOrder(int.MaxValue)
            .AllowAnonymous()
            .ExcludeFromDescription();

        return endpoints;
    }

    /// <summary>
    /// Root path of an embedded file, with <c>?v=</c> set to its content hash when the file is in the assembly.
    /// A request that carries <c>v</c> is cached for a year. The same path without it is revalidated.
    /// </summary>
    /// <param name="assembly">The assembly that embeds the file.</param>
    /// <param name="relativePath">Path under <c>wwwroot</c>, such as <c>css/site.css</c>.</param>
    /// <returns>A root-absolute URL.</returns>
    public static string Url(Assembly assembly, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        string name = assembly.GetName().Name ?? "unknown";
        string relative = (relativePath ?? string.Empty).Replace('\\', '/').Trim('/');
        string root = $"/_content/{name}/{relative}";
        return TryNormalize(relative, out string key) && Catalog(assembly).TryGetValue(key, out var asset)
            ? $"{root}?v={asset.Version}"
            : root;
    }

    /// <summary>
    /// The named assembly if it is already loaded, otherwise the dll sitting beside <paramref name="neighbor"/>.
    /// Plugin hosts keep companion assemblies in one folder, and a normal app output does the same.
    /// </summary>
    /// <param name="assemblyName">The file name without <c>.dll</c>.</param>
    /// <param name="neighbor">An assembly already loaded from that folder.</param>
    /// <returns>The assembly, or null when it cannot be found.</returns>
    public static Assembly? Resolve(string assemblyName, Assembly neighbor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        ArgumentNullException.ThrowIfNull(neighbor);

        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName, StringComparison.Ordinal));
        if (loaded is not null)
        {
            return loaded;
        }

        string? directory = Path.GetDirectoryName(neighbor.Location);
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        string path = Path.Combine(directory, assemblyName + ".dll");
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }

    private static IResult Serve(Assembly assembly, string? path, HttpContext context)
    {
        if (!TryNormalize(path, out string relative) || !Catalog(assembly).TryGetValue(relative, out var asset))
        {
            return Results.NotFound();
        }

        var headers = context.Response.GetTypedHeaders();
        headers.ETag = asset.ETag;
        headers.CacheControl = context.Request.Query.ContainsKey("v")
            ? new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromDays(365), Extensions = { new NameValueHeaderValue("immutable") } }
            : new CacheControlHeaderValue { NoCache = true };

        var requestETags = context.Request.GetTypedHeaders().IfNoneMatch;
        return requestETags.Any(tag => tag.Compare(asset.ETag, useStrongComparison: false))
            ? Results.StatusCode(StatusCodes.Status304NotModified)
            : Results.Bytes(asset.Content, asset.ContentType);
    }

    private static IReadOnlyDictionary<string, Asset> Catalog(Assembly assembly) =>
        Catalogs.GetOrAdd(assembly, Load);

    private static IReadOnlyDictionary<string, Asset> Load(Assembly assembly)
    {
        string? name = assembly.GetName().Name;
        if (string.IsNullOrEmpty(name))
        {
            return new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        }

        string prefix = name + Folder;
        var found = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            if (!resource.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            string relative = resource[prefix.Length..].Replace('\\', '/');
            if (!TryNormalize(relative, out string key))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resource);
            if (stream is null)
            {
                continue;
            }

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            byte[] content = buffer.ToArray();
            string version = Convert.ToHexStringLower(SHA256.HashData(content))[..12];
            found[key] = new Asset(content, ContentType(key), new EntityTagHeaderValue($"\"{version}\""), version);
        }

        return found;
    }

    private static bool TryNormalize(string? path, out string relative)
    {
        relative = (path ?? string.Empty).Replace('\\', '/').Trim('/');
        return relative.Length > 0
            && relative.IndexOf('\0') < 0
            && !relative.Contains("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relative);
    }

    private static string ContentType(string relative) => Path.GetExtension(relative).ToLowerInvariant() switch
    {
        ".css" => "text/css; charset=utf-8",
        ".js" => "text/javascript; charset=utf-8",
        ".map" => "application/json",
        ".json" => "application/json",
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".ico" => "image/x-icon",
        ".woff" => "font/woff",
        ".woff2" => "font/woff2",
        ".mp3" => "audio/mpeg",
        ".txt" => "text/plain; charset=utf-8",
        ".wasm" => "application/wasm",
        _ => "application/octet-stream",
    };

    private sealed record Asset(byte[] Content, string ContentType, EntityTagHeaderValue ETag, string Version);
}
