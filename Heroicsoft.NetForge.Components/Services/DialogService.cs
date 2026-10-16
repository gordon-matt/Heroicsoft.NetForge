namespace Heroicsoft.NetForge.Components.Services;

/// <summary>
/// Awaitable confirm and prompt dialogs mounted by <c>ForgeDialogHost</c>. Pages inject this
/// instead of holding a component reference.
/// </summary>
public sealed class DialogService
{
    private ConfirmDialog? confirm;
    private PromptDialog? prompt;

    internal void Attach(ConfirmDialog dialog) => confirm = dialog;

    internal void Attach(PromptDialog dialog) => prompt = dialog;

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
        string initialValue = "",
        string confirmLabel = "OK",
        bool selectFileNameStem = false) =>
        Require(prompt).ShowAsync(title, label, initialValue, confirmLabel, selectFileNameStem);

    private static T Require<T>(T? dialog) where T : class =>
        dialog ?? throw new InvalidOperationException(
            "The dialog host is not mounted. Render AppShell, EmptyLayout, or ForgeDialogHost before prompting.");
}