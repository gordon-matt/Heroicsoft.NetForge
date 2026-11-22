using Heroicsoft.NetForge.Components.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

/// <summary>
/// Renders the toast stack, <see cref="ConfirmDialog"/>, and <see cref="PromptDialog"/>, and attaches
/// those dialogs so a page can show them through the shared dialog service.
/// </summary>
public partial class ForgeDialogHost
{
    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private ConfirmDialog confirmDialog = default!;
    private PromptDialog promptDialog = default!;

    /// <summary>
    /// Attaches the confirm and prompt dialogs after every render so a later call can show them.
    /// </summary>
    /// <param name="firstRender">Ignored. The dialogs are attached on every render.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        Dialogs.Attach(confirmDialog);
        Dialogs.Attach(promptDialog);
    }
}