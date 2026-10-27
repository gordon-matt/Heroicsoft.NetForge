using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Charts;

/// <summary>
/// Draws a Chart.js chart from a configuration object built in C#.
/// </summary>
/// <remarks>
/// The host page has to load <c>chart.umd.js</c> and then <c>charts.js</c> from this package
/// (<c>_content/Heroicsoft.NetForge.Components/js/</c>). Colours and fonts are read from the
/// active stylesheet, so a theme change restyles charts that are already on the page.
/// </remarks>
public partial class Chart : IAsyncDisposable
{
    private readonly string canvasId = $"nf-chart-{Guid.NewGuid():N}";

    private string? renderedSignature;

    private bool isRendered;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Parameter, EditorRequired]
    public object Config { get; set; } = default!;

    /// <summary><c>sm</c> is 220px, <c>lg</c> is 380px, and anything else is 300px.</summary>
    [Parameter]
    public string Height { get; set; } = "md";

    private int HeightPx => Height switch
    {
        "sm" => 220,
        "lg" => 380,
        _ => 300,
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Chart.js redraws are expensive, and Blazor re-renders on unrelated state changes, so
        // the config is only pushed across when it has actually changed.
        string signature = JsonSerializer.Serialize(Config);

        if (signature == renderedSignature)
        {
            return;
        }

        renderedSignature = signature;
        isRendered = true;

        await Js.InvokeVoidAsync("netForge.renderChart", canvasId, Config);
    }

    public async ValueTask DisposeAsync()
    {
        if (!isRendered)
        {
            return;
        }

        try
        {
            await Js.InvokeVoidAsync("netForge.destroyChart", canvasId);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone, so the chart went with the page.
        }
    }
}
