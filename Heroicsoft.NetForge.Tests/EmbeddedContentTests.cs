using System.Net;
using System.Reflection;
using Heroicsoft.NetForge.WebAssets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Heroicsoft.NetForge.Tests;

public sealed class EmbeddedContentTests
{
    [Fact]
    public void Url_includes_the_content_hash()
    {
        string url = EmbeddedContent.Url(typeof(EmbeddedContentTests).Assembly, "js/nested.txt");

        Assert.StartsWith("/_content/Heroicsoft.NetForge.Tests/js/nested.txt?v=", url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Embedded_wwwroot_is_served_at_the_content_path()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, "http://127.0.0.1:0");
        var app = builder.Build();
        app.MapEmbeddedContent(typeof(EmbeddedContentTests).Assembly);
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        try
        {
            var probe = await client.GetAsync("/_content/Heroicsoft.NetForge.Tests/probe.txt");
            Assert.Equal(HttpStatusCode.OK, probe.StatusCode);
            Assert.Equal("probe", (await probe.Content.ReadAsStringAsync()).Trim());
            Assert.Equal("no-cache", probe.Headers.CacheControl?.ToString());

            string versioned = EmbeddedContent.Url(Assembly.GetExecutingAssembly(), "probe.txt");
            var cached = await client.GetAsync(versioned);
            Assert.Equal(HttpStatusCode.OK, cached.StatusCode);
            Assert.Contains("immutable", cached.Headers.CacheControl?.ToString(), StringComparison.Ordinal);

            var nested = await client.GetAsync("/_content/Heroicsoft.NetForge.Tests/js/nested.txt");
            Assert.Equal(HttpStatusCode.OK, nested.StatusCode);
            Assert.Equal("nested", (await nested.Content.ReadAsStringAsync()).Trim());

            var etag = probe.Headers.ETag;
            Assert.NotNull(etag);
            var again = new HttpRequestMessage(HttpMethod.Get, "/_content/Heroicsoft.NetForge.Tests/probe.txt");
            again.Headers.IfNoneMatch.Add(etag);
            var revalidated = await client.SendAsync(again);
            Assert.Equal(HttpStatusCode.NotModified, revalidated.StatusCode);
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
