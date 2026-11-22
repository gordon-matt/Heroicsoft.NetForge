using Heroicsoft.NetForge.Components.Dialogs;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

/// <summary>
/// Renders a centered Bootstrap modal that asks the user to confirm or cancel.
/// Nothing is on screen until <see cref="ShowAsync"/> is called. Cancel and the close button answer no.
/// </summary>
/// <remarks>The backdrop does not close the dialog.</remarks>
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
    /// <param name="title">Text in the modal header.</param>
    /// <param name="message">Paragraph in the modal body.</param>
    /// <param name="confirmLabel">Label of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="isDestructive">
    /// When <see langword="true"/>, the confirm button uses <c>btn-danger</c>. When <see langword="false"/>,
    /// the default, it uses <c>btn-primary</c>.
    /// </param>
    /// <param name="detail">
    /// Secondary sentence under <paramref name="message"/>. <see langword="null"/> omits it.
    /// </param>
    /// <returns>
    /// A task that completes with <see langword="true"/> when the confirm button is clicked, or
    /// <see langword="false"/> when Cancel or the close button is clicked.
    /// </returns>
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