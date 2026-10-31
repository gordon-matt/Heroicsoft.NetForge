using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Wizards;

/// <summary>
/// One step inside <see cref="Wizard"/>. Forward navigation calls <see cref="OnLeaving"/> and stays
/// on this step when it returns false. Moving backward does not call it.
/// </summary>
public partial class WizardStep : ComponentBase, IDisposable
{
    [CascadingParameter]
    private Wizard? Parent { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Return false to keep the wizard on this step.</summary>
    [Parameter]
    public Func<Task<bool>>? OnLeaving { get; set; }

    internal int Index { get; private set; }

    private bool Visible => Parent is not null && Parent.ActiveIndex == Index;

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(WizardStep)} must be placed inside a {nameof(Wizard)}.");
        }

        Index = Parent.Register(this);
    }

    public void Dispose() => Parent?.Unregister(this);
}
