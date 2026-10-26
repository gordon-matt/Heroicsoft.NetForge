using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Dialogs;

public partial class PromptDialog : IPromptDialog, IDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private readonly string inputId = "nf-prompt-" + Guid.NewGuid().ToString("N");
    private readonly string titleId = "nf-prompt-title-" + Guid.NewGuid().ToString("N");
    private bool isVisible;
    private string title = string.Empty;
    private string label = string.Empty;
    private string confirmLabel = "OK";
    private string? description;
    private int maxLength = 256;
    private string? error;
    private bool selectFileNameStem;
    private ElementReference inputElement;
    private bool shouldFocus;
    private TaskCompletionSource<string?>? completion;

    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Shows the prompt and completes with the trimmed text, or null when cancelled.
    /// When <paramref name="selectFileNameStem"/> is true, the selection stops before the extension.
    /// </summary>
    public Task<string?> ShowAsync(
        string title,
        string label,
        string? initialValue = "",
        string confirmLabel = "OK",
        bool selectFileNameStem = false,
        string? description = null,
        int maxLength = 256)
    {
        completion?.TrySetResult(null);

        this.title = title;
        this.label = label;
        this.confirmLabel = confirmLabel;
        this.selectFileNameStem = selectFileNameStem;
        this.description = description;
        this.maxLength = maxLength;

        Value = initialValue ?? "";
        error = null;
        isVisible = true;
        shouldFocus = true;

        completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        StateHasChanged();

        return completion.Task;
    }

    /// <summary>Shows a validation message without closing the dialog.</summary>
    public void ShowError(string message)
    {
        error = message;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!shouldFocus)
        {
            return;
        }

        shouldFocus = false;

        try
        {
            if (selectFileNameStem)
            {
                await JS.InvokeVoidAsync("netForge.selectFileNameStem", inputElement);
            }
            else
            {
                await inputElement.FocusAsync();
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Focus is a nicety. The circuit may be gone, or the dialog closed before this ran.
        }
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            Cancel();
        }
    }

    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return;
        }

        isVisible = false;
        completion?.TrySetResult(Value.Trim());
        completion = null;
    }

    private void Cancel()
    {
        isVisible = false;
        completion?.TrySetResult(null);
        completion = null;
    }

    public void Dispose() => completion?.TrySetResult(null);
}