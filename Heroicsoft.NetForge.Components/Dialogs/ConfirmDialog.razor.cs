using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>
/// Renders a modal that asks the user to confirm or cancel. Escape and a backdrop click count as cancel.
/// </summary>
public partial class ConfirmDialog : IConfirmDialog, IDisposable
{
    private readonly string titleId = "nf-confirm-title-" + Guid.NewGuid().ToString("N");
    private readonly string messageId = "nf-confirm-message-" + Guid.NewGuid().ToString("N");
    private bool isVisible;
    private bool isDestructive;
    private bool shouldFocus;
    private string title = string.Empty;
    private string message = string.Empty;
    private string? detail;
    private string confirmLabel = "OK";
    private ElementReference cancelButton;
    private ElementReference confirmButton;
    private TaskCompletionSource<bool>? completion;

    /// <summary>
    /// Shows the dialog and completes when the user answers. Awaiting the answer keeps the
    /// caller's logic in one place rather than split across an event handler.
    /// A later call completes the previous task with <see langword="false"/>.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="message">Main paragraph.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="isDestructive">Styles the confirm button as danger and moves focus to Cancel, so Enter does not confirm.</param>
    /// <param name="detail">Second paragraph. <see langword="null"/> omits it.</param>
    /// <returns><see langword="true"/> when the user confirms; <see langword="false"/> when the user cancels, dismisses the dialog, or a newer call replaces it.</returns>
    public Task<bool> ShowAsync(
        string title,
        string message,
        string confirmLabel = "OK",
        bool isDestructive = false,
        string? detail = null)
    {
        // A second dialog replaces the first; the first caller gets "no" rather than waiting forever.
        completion?.TrySetResult(false);

        this.title = title;
        this.message = message;
        this.detail = detail;
        this.confirmLabel = confirmLabel;
        this.isDestructive = isDestructive;

        isVisible = true;
        shouldFocus = true;
        completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        StateHasChanged();

        return completion.Task;
    }

    /// <summary>
    /// Moves focus to Cancel when the dialog is destructive, otherwise to the confirm button. A failure to focus leaves the dialog usable.
    /// </summary>
    /// <param name="firstRender">Ignored. Focus runs only after the dialog opens.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!shouldFocus)
        {
            return;
        }

        shouldFocus = false;

        try
        {
            // The safe answer is focused for destructive actions, so Enter can't delete by accident.
            await (isDestructive ? cancelButton : confirmButton).FocusAsync();
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
            Close(false);
        }
    }

    private void Close(bool confirmed)
    {
        isVisible = false;
        completion?.TrySetResult(confirmed);
        completion = null;
    }

    /// <summary>
    /// Completes a pending answer with <see langword="false"/> when the dialog is removed while it is still open.
    /// </summary>
    public void Dispose() => completion?.TrySetResult(false);
}