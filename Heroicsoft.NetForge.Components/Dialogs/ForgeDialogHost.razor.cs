using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Dialogs;

/// <summary>
/// Renders the toaster, confirm dialog, and prompt dialog, and attaches them to <see cref="DialogService"/> after each render.
/// </summary>
public partial class ForgeDialogHost
{
    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private ConfirmDialog confirmDialog = default!;
    private PromptDialog promptDialog = default!;

    /// <summary>
    /// Attaches the confirm and prompt dialogs so <see cref="DialogService"/> can show them.
    /// </summary>
    /// <param name="firstRender">Ignored. The dialogs are attached after every render.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        Dialogs.Attach(confirmDialog);
        Dialogs.Attach(promptDialog);
    }
}