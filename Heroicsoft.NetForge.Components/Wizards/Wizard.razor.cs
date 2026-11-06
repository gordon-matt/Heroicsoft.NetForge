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
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the orientation.
    /// </summary>
    [Parameter]
    public WizardOrientation Orientation { get; set; } = WizardOrientation.Vertical;

    /// <summary>
    /// Gets or sets the finish text.
    /// </summary>
    [Parameter]
    public string FinishText { get; set; } = "Finish";

    /// <summary>
    /// Gets or sets a value indicating whether this instance is busy.
    /// </summary>
    [Parameter]
    public bool Busy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is show cancel.
    /// </summary>
    [Parameter]
    public bool ShowCancel { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the cancel event.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Gets or sets the callback raised for the finished event.
    /// </summary>
    [Parameter]
    public EventCallback OnFinished { get; set; }

    internal int ActiveIndex { get; private set; }

    internal int Register(WizardStep step)
    {
        steps.Add(step);
        return steps.Count - 1;
    }

    /// <inheritdoc/>
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
