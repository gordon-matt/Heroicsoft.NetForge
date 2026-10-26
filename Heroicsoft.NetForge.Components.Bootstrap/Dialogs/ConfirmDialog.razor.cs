using Heroicsoft.NetForge.Components.Dialogs;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

public partial class ConfirmDialog : IConfirmDialog
{
    private bool isVisible;
    private string title = string.Empty;
    private string message = string.Empty;
    private string? detail;
    private string confirmLabel = "OK";
    private string confirmButtonClass = "btn-primary";
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
        this.title = title;
        this.message = message;
        this.detail = detail;
        this.confirmLabel = confirmLabel;
        confirmButtonClass = isDestructive ? "btn-danger" : "btn-primary";

        isVisible = true;
        completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        StateHasChanged();

        return completion.Task;
    }

    private void Close(bool confirmed)
    {
        isVisible = false;
        completion?.TrySetResult(confirmed);
        completion = null;
    }
}