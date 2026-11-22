using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Wizards;

/// <summary>
/// Step navigation matching the old jQuery Steps behaviour: a step list, free movement backward,
/// and a gate on the current step before any forward move.
/// </summary>
public partial class Wizard
{
    private readonly List<WizardStep> steps = [];

    /// <summary>
    /// The <see cref="WizardStep"/> components. Only the active step's panel is visible.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Where the step list sits. Defaults to <see cref="WizardOrientation.Vertical"/>, down the side. <see cref="WizardOrientation.Horizontal"/> places it across the top.
    /// </summary>
    [Parameter]
    public WizardOrientation Orientation { get; set; } = WizardOrientation.Vertical;

    /// <summary>
    /// Label of the button on the last step. Defaults to <c>Finish</c>. Earlier steps show Next.
    /// </summary>
    [Parameter]
    public string FinishText { get; set; } = "Finish";

    /// <summary>
    /// Disables the step list and the Previous, Next, Finish, and Cancel buttons, and ignores navigation. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Busy { get; set; }

    /// <summary>
    /// Shows a Cancel button when <see cref="OnCancel"/> has a delegate. Defaults to <see langword="false"/>, which hides it.
    /// </summary>
    [Parameter]
    public bool ShowCancel { get; set; }

    /// <summary>
    /// Raised with no argument when Cancel is clicked. The button is hidden when this callback has no delegate.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Raised with no argument when Finish is clicked, after the current step allows the move. Does not fire when <see cref="Busy"/> is <see langword="true"/> or the step's leaving callback returns <see langword="false"/>.
    /// </summary>
    [Parameter]
    public EventCallback OnFinished { get; set; }

    internal int ActiveIndex { get; private set; }

    internal int Register(WizardStep step)
    {
        steps.Add(step);
        return steps.Count - 1;
    }

    /// <summary>
    /// Asks for another render after the first one, so steps that registered during that render appear in the step list.
    /// </summary>
    /// <param name="firstRender"><see langword="true"/> on the render that first builds the step list.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            StateHasChanged();
        }
    }

    internal void Unregister(WizardStep step)
    {
        int index = steps.IndexOf(step);
        if (index < 0)
        {
            return;
        }

        steps.RemoveAt(index);
        if (ActiveIndex >= steps.Count)
        {
            ActiveIndex = Math.Max(0, steps.Count - 1);
        }
    }

    private Task PreviousAsync() => GoToAsync(ActiveIndex - 1);

    private Task NextAsync() => GoToAsync(ActiveIndex + 1);

    private async Task GoToAsync(int index)
    {
        if (Busy || index == ActiveIndex || index < 0 || index >= steps.Count)
        {
            return;
        }

        if (index > ActiveIndex && !await LeaveCurrentAsync())
        {
            return;
        }

        ActiveIndex = index;
    }

    private async Task FinishAsync()
    {
        if (Busy || !await LeaveCurrentAsync())
        {
            return;
        }

        await OnFinished.InvokeAsync();
    }

    private async Task<bool> LeaveCurrentAsync()
    {
        if (ActiveIndex < 0 || ActiveIndex >= steps.Count)
        {
            return true;
        }

        var onLeaving = steps[ActiveIndex].OnLeaving;
        return onLeaving is null || await onLeaving();
    }
}
