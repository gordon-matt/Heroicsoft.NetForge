using Heroicsoft.NetForge.Components.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Dialogs;

public partial class ForgeDialogHost
{
    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private ConfirmDialog confirmDialog = default!;
    private PromptDialog promptDialog = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        Dialogs.Attach(confirmDialog);
        Dialogs.Attach(promptDialog);
    }
}