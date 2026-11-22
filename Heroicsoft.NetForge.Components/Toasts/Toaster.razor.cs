using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Toasts;

/// <summary>
/// Renders the toasts from <see cref="ToastService"/> and plays a sound when <see cref="ForgeToastSoundOptions"/> has one for the tone.
/// </summary>
public partial class Toaster : IDisposable
{
    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<ForgeToastSoundOptions> SoundOptions { get; set; } = default!;

    /// <summary>Subscribes so a new or dismissed toast redraws this list, and a new toast can play a sound.</summary>
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
        "success" => "check-circle",
        "danger" => "danger",
        "warning" => "warning",
        _ => "info",
    };

    /// <summary>Stops redrawing and playing sounds for later toasts.</summary>
    public void Dispose()
    {
        ToastService.Changed -= OnToastsChanged;
        ToastService.Shown -= OnToastShown;
    }
}