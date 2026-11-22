using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Bootstrap.Toasts;

/// <summary>
/// Renders the current toasts in a fixed stack at the top end of the page. Each toast uses
/// <c>text-bg-</c> plus its level, a Bootstrap icon for that level, and a close button.
/// Success, danger, and warning have their own icons; any other level uses the info icon.
/// </summary>
public partial class Toaster : IDisposable
{
    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<ForgeToastSoundOptions> SoundOptions { get; set; } = default!;

    /// <summary>
    /// Subscribes to toast changes so the stack re-renders, and plays the configured sound when a toast is shown.
    /// A missing script or a disconnected circuit stays silent.
    /// </summary>
    protected override void OnInitialized()
    {
        ToastService.Changed += OnToastsChanged;
        ToastService.Shown += OnToastShown;
    }

    private void OnToastsChanged() => InvokeAsync(StateHasChanged);

    private void OnToastShown(ToastService.Toast toast)
    {
        string? sound = SoundOptions.Value.SoundFor(toast.Level);
        if (sound is null)
        {
            return;
        }

        _ = InvokeAsync(async () =>
        {
            try
            {
                await JS.InvokeVoidAsync("netForge.playSound", sound);
            }
            catch (Exception ex) when (ex is JSException or InvalidOperationException or JSDisconnectedException or TaskCanceledException)
            {
                // Prerendering, a dropped circuit, or a page without the NetForge script: stay silent.
            }
        });
    }

    private static string IconFor(string level) => level switch
    {
        "success" => "bi-check-circle",
        "danger" => "bi-exclamation-octagon",
        "warning" => "bi-exclamation-triangle",
        _ => "bi-info-circle",
    };

    /// <summary>
    /// Stops listening for toasts to appear or change.
    /// </summary>
    public void Dispose()
    {
        ToastService.Changed -= OnToastsChanged;
        ToastService.Shown -= OnToastShown;
    }
}