namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>A confirm dialog the host has mounted. Both component sets implement it.</summary>
public interface IConfirmDialog
{
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

    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmLabel = "OK",
        bool isDestructive = false,
        string? detail = null) =>
        Require(confirm).ShowAsync(title, message, confirmLabel, isDestructive, detail);

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