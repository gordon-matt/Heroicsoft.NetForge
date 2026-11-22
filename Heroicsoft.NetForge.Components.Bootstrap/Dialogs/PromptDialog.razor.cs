using Heroicsoft.NetForge.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

/// <summary>
/// Renders a centered Bootstrap modal with a single-line text box.
/// Nothing is on screen until <see cref="ShowAsync"/> is called. Cancel and the close button complete
/// with <see langword="null"/>. A blank value cannot be submitted.
/// </summary>
/// <remarks>The backdrop does not close the dialog.</remarks>
public partial class PromptDialog : IPromptDialog
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private readonly string inputId = "bf-prompt-" + Guid.NewGuid().ToString("N");
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
    /// Text currently in the prompt box. <see cref="ShowAsync"/> sets it from the initial value, and the box updates it on each keystroke.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Shows the prompt and completes with the trimmed text, or <see langword="null"/> when cancelled.
    /// When <paramref name="selectFileNameStem"/> is <see langword="true"/>, the selection stops before the extension.
    /// </summary>
    /// <param name="title">Text in the modal header.</param>
    /// <param name="label">Label above the text box.</param>
    /// <param name="initialValue">Text placed in the box. <see langword="null"/> starts empty. Defaults to empty.</param>
    /// <param name="confirmLabel">
    /// Label of the submit button. Defaults to <c>OK</c>. The button stays disabled while the box is blank.
    /// </param>
    /// <param name="selectFileNameStem">
    /// When <see langword="true"/>, focuses the box and selects the file name without its extension.
    /// When <see langword="false"/>, the default, only moves focus to the box.
    /// </param>
    /// <param name="description">Secondary sentence above the field. <see langword="null"/> omits it.</param>
    /// <param name="maxLength">Maximum length of the box, written to <c>maxlength</c>. Defaults to 256.</param>
    /// <returns>
    /// A task that completes with the trimmed text, or <see langword="null"/> when the user cancels.
    /// Submit is ignored while the box is blank.
    /// </returns>
    public Task<string?> ShowAsync(
        string title,
        string label,
        string? initialValue = "",
        string confirmLabel = "OK",
        bool selectFileNameStem = false,
        string? description = null,
        int maxLength = 256)
    {
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
    /// <param name="message">The message, shown in small danger text under the box.</param>
    public void ShowError(string message)
    {
        error = message;
        StateHasChanged();
    }

    /// <summary>
    /// Focuses the text box after the dialog opens. When the prompt was asked to select a file name
    /// stem, selects the name up to the extension instead of only moving focus.
    /// </summary>
    /// <param name="firstRender">Ignored. Focus runs when the dialog has just been shown.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!shouldFocus)
        {
            return;
        }

        shouldFocus = false;

        if (selectFileNameStem)
        {
            await JS.InvokeVoidAsync("netForge.selectFileNameStem", inputElement);
        }
        else
        {
            await inputElement.FocusAsync();
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
}