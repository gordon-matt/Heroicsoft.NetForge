namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>A confirm dialog the host has mounted. Both component sets implement it.</summary>
public interface IConfirmDialog
{
    /// <summary>
    /// Gets the show.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="confirmLabel">The confirm label.</param>
    /// <param name="isDestructive">The is destructive.</param>
    /// <param name="detail">The detail.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    Task<bool> ShowAsync(
        string title,
        string message,
        string confirmLabel = "OK",
        bool isDestructive = false,
        string? detail = null);
}

/// <summary>A single-line prompt dialog the host has mounted. Both component sets implement it.</summary>
public interface IPromptDialog
{
    /// <summary>
    /// Gets the show.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="label">The label.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="confirmLabel">The confirm label.</param>
    /// <param name="selectFileNameStem">The select file name stem.</param>
    /// <param name="description">The description.</param>
    /// <param name="maxLength">The max length.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    Task<string?> ShowAsync(
        string title,
        string label,
        string? initialValue = "",
        string confirmLabel = "OK",
        bool selectFileNameStem = false,
        string? description = null,
        int maxLength = 256);
}

/// <summary>
/// Awaitable confirm and prompt dialogs mounted by the dialog host component. Pages inject this
/// instead of holding a component reference, so the same page code runs on either component set.
/// </summary>
public sealed class DialogService
{
    private IConfirmDialog? confirm;
    private IPromptDialog? prompt;

    /// <summary>Called by the dialog host once its dialogs have rendered.</summary>
    public void Attach(IConfirmDialog dialog) => confirm = dialog;

    /// <summary>Called by the dialog host once its dialogs have rendered.</summary>
    public void Attach(IPromptDialog dialog) => prompt = dialog;

    /// <summary>
    /// Gets the confirm.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="confirmLabel">The confirm label.</param>
    /// <param name="isDestructive">The is destructive.</param>
    /// <param name="detail">The detail.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmLabel = "OK",
        bool isDestructive = false,
        string? detail = null) =>
        Require(confirm).ShowAsync(title, message, confirmLabel, isDestructive, detail);

    /// <summary>
    /// Gets the prompt.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="label">The label.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="confirmLabel">The confirm label.</param>
    /// <param name="selectFileNameStem">The select file name stem.</param>
    /// <param name="description">The description.</param>
    /// <param name="maxLength">The max length.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    public Task<string?> PromptAsync(
        string title,
        string label,
        string? initialValue = "",
        string confirmLabel = "OK",
        bool selectFileNameStem = false,
        string? description = null,
        int maxLength = 256) =>
        Require(prompt).ShowAsync(title, label, initialValue, confirmLabel, selectFileNameStem, description, maxLength);

    private static T Require<T>(T? dialog) where T : class =>
        dialog ?? throw new InvalidOperationException(
            "The dialog host is not mounted. Render the shell, the empty layout, or the dialog host before prompting.");
}