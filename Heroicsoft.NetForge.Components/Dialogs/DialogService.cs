namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>A confirm dialog the host has mounted. Both component sets implement it.</summary>
public interface IConfirmDialog
{
    /// <summary>
    /// Shows the confirm dialog and completes when the user answers.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="message">Main paragraph.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="isDestructive">Styles the confirm button as a destructive action.</param>
    /// <param name="detail">Second paragraph under the message. <see langword="null"/> omits it.</param>
    /// <returns><see langword="true"/> when the user confirms; <see langword="false"/> when the user cancels.</returns>
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
    /// Shows the prompt and completes with the trimmed text, or <see langword="null"/> when the user cancels. Blank text cannot be submitted.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="label">Label of the text field.</param>
    /// <param name="initialValue">Text the field opens with. <see langword="null"/> is an empty field. Defaults to empty.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="selectFileNameStem">When <see langword="true"/>, the initial selection stops before the file extension.</param>
    /// <param name="description">Paragraph under the heading. <see langword="null"/> omits it.</param>
    /// <param name="maxLength">Maximum characters the field accepts. Defaults to 256.</param>
    /// <returns>The trimmed text, or <see langword="null"/> when cancelled.</returns>
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
    /// Shows the confirm dialog and completes when the user answers.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="message">Main paragraph.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="isDestructive">Styles the confirm button as a destructive action.</param>
    /// <param name="detail">Second paragraph under the message. <see langword="null"/> omits it.</param>
    /// <returns><see langword="true"/> when the user confirms; <see langword="false"/> when the user cancels.</returns>
    /// <exception cref="InvalidOperationException">The dialog host is not mounted.</exception>
    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmLabel = "OK",
        bool isDestructive = false,
        string? detail = null) =>
        Require(confirm).ShowAsync(title, message, confirmLabel, isDestructive, detail);

    /// <summary>
    /// Shows the prompt and completes with the trimmed text, or <see langword="null"/> when the user cancels. Blank text cannot be submitted.
    /// </summary>
    /// <param name="title">Dialog heading.</param>
    /// <param name="label">Label of the text field.</param>
    /// <param name="initialValue">Text the field opens with. <see langword="null"/> is an empty field. Defaults to empty.</param>
    /// <param name="confirmLabel">Text of the confirm button. Defaults to <c>OK</c>.</param>
    /// <param name="selectFileNameStem">When <see langword="true"/>, the initial selection stops before the file extension.</param>
    /// <param name="description">Paragraph under the heading. <see langword="null"/> omits it.</param>
    /// <param name="maxLength">Maximum characters the field accepts. Defaults to 256.</param>
    /// <returns>The trimmed text, or <see langword="null"/> when cancelled.</returns>
    /// <exception cref="InvalidOperationException">The dialog host is not mounted.</exception>
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