using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Wizards;

/// <summary>
/// Step navigation matching the old jQuery Steps behaviour: a step list, free movement backward,
/// and a gate on the current step before any forward move.
/// </summary>
public partial class Wizard
{
    private readonly List<WizardStep> steps = [];

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public WizardOrientation Orientation { get; set; } = WizardOrientation.Vertical;

    [Parameter]
    public string FinishText { get; set; } = "Finish";

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public bool ShowCancel { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback OnFinished { get; set; }

    internal int ActiveIndex { get; private set; }

    internal int Register(WizardStep step)
    {
        steps.Add(step);
        return steps.Count - 1;
    }

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

        Func<Task<bool>>? onLeaving = steps[ActiveIndex].OnLeaving;
        if (onLeaving is null)
        {
            return true;
        }

        return await onLeaving();
    }
}
