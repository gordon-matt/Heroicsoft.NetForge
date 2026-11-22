using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>
/// Renders a single-line text prompt. Cancel, Escape, and a backdrop click complete with <see langword="null"/>.
/// </summary>
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

    /// <summary>
    /// Text in the field. <c>ShowAsync</c> sets it from the initial value, and typing updates it. Whitespace alone cannot be submitted.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Shows the prompt and completes with the trimmed text, or <see langword="null"/> when cancelled.
    /// When <paramref name="selectFileNameStem"/> is <see langword="true"/>, the selection stops before the extension.
    /// A later call completes the previous task with <see langword="null"/>.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="label">Label of the text field.</param>
    /// <param name="initialValue">Text the field opens with. <see langword="null"/> is an empty field. Defaults to empty.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="selectFileNameStem">When <see langword="true"/>, the initial selection stops before the file extension.</param>
    /// <param name="description">Paragraph under the heading. <see langword="null"/> omits it.</param>
    /// <param name="maxLength">Maximum characters the field accepts. Defaults to 256.</param>
    /// <returns>The trimmed text, or <see langword="null"/> when cancelled.</returns>
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

    /// <summary>
    /// Focuses the field after the prompt opens. When the file-name stem should be selected, selects up to the extension instead of only focusing. A failure to focus leaves the dialog usable.
    /// </summary>
    /// <param name="firstRender">Ignored. Focus runs only after the prompt opens.</param>
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

    /// <summary>
    /// Completes a pending prompt with <see langword="null"/> when the dialog is removed while it is still open.
    /// </summary>
    public void Dispose() => completion?.TrySetResult(null);
}