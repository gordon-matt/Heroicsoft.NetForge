using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Toasts;

public partial class Toaster : IDisposable
{
    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<ForgeToastSoundOptions> SoundOptions { get; set; } = default!;

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

    public void Dispose()
    {
        ToastService.Changed -= OnToastsChanged;
        ToastService.Shown -= OnToastShown;
    }
}