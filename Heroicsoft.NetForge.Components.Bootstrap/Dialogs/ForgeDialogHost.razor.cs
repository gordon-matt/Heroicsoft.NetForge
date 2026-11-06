using Heroicsoft.NetForge.Components.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

/// <summary>
/// Represents the forge dialog host.
/// </summary>
public partial class ForgeDialogHost
{
    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private ConfirmDialog confirmDialog = default!;
    private PromptDialog promptDialog = default!;

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        Dialogs.Attach(confirmDialog);
        Dialogs.Attach(promptDialog);
    }
}