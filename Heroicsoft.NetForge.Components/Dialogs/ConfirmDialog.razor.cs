using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Dialogs;

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
    /// </summary>
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

    public void Dispose() => completion?.TrySetResult(false);
}