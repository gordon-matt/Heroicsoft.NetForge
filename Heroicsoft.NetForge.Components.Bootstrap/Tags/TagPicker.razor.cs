using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Tags;

/// <summary>
/// Selected values as chips, plus a text field that suggests matches and can commit a new name.
/// The parent resolves that name into a real item and adds it to <see cref="Selected"/>.
/// Enter commits the typed name, and Escape hides the suggestion menu. An empty selection shows
/// an italic sentence that no items of <see cref="Label"/> exist yet.
/// </summary>
/// <typeparam name="TItem">The type of one selected item. Must be a reference type.</typeparam>
/// <remarks>
/// The suggestion menu hides shortly after the box loses focus, so a click on a suggestion can land first.
/// </remarks>
public partial class TagPicker<TItem> : IDisposable
    where TItem : class
{
    private CancellationTokenSource? blurCts;
    private string? input;
    private CancellationTokenSource? searchCts;
    private bool showSuggestions;
    private IReadOnlyList<TItem> suggestions = [];

    /// <summary>
    /// When <see langword="false"/>, typed text can only pick an existing suggestion. When
    /// <see langword="true"/>, the default, a Create row is shown when the typed name is not already suggested.
    /// </summary>
    [Parameter]
    public bool AllowCreate { get; set; } = true;

    /// <summary>
    /// Extra CSS classes on each selected chip, which already has <c>chip bf-tag</c>. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? ChipClass { get; set; }

    /// <summary>
    /// Extra CSS classes on the <c>chip-input bf-tag-picker</c> wrapper. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Id of the text box. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Singular name of an item. The placeholder reads "Add" plus this name, and an empty selection
    /// reads "No" plus this name "yet." The name is lower-cased. Defaults to <c>Item</c>.
    /// </summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = "Item";

    /// <summary>
    /// Display name for an item, shown on chips and in the suggestion menu. Required because the picker is generic.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> NameOf { get; set; } = _ => string.Empty;

    /// <summary>
    /// Called with the chosen name. The parent adds the resolved item to <see cref="Selected"/>.
    /// A blank name is ignored. A name that matches a selected item, ignoring case, clears the box and does not call this delegate.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<string, Task> OnAddAsync { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Invoked after a name is committed or a chip is removed. Not invoked when the typed name is blank or already selected.
    /// </summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }

    /// <summary>
    /// Suggestions for the current text. Called as the user types and when the box receives focus,
    /// with the box text, or empty when the box is blank. Already selected names are hidden, and at most ten matches are shown.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<string, Task<IReadOnlyList<TItem>>> SearchAsync { get; set; } =
        _ => Task.FromResult<IReadOnlyList<TItem>>([]);

    /// <summary>
    /// Chips already chosen. An empty list shows the empty message. Removing a chip removes that
    /// item from this list and raises <see cref="OnChanged"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public IList<TItem> Selected { get; set; } = [];

    private string Placeholder => $"Add {Label.ToLowerInvariant()}…";

    /// <summary>
    /// Cancels any pending suggestion refresh or blur hide and releases those cancellation sources.
    /// </summary>
    public void Dispose()
    {
        blurCts?.Cancel();
        blurCts?.Dispose();
        searchCts?.Cancel();
        searchCts?.Dispose();
    }

    private async Task CommitAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (Selected.Any(item => string.Equals(NameOf(item), name, StringComparison.OrdinalIgnoreCase)))
        {
            input = string.Empty;
            return;
        }

        await OnAddAsync(name);
        input = string.Empty;
        suggestions = [];
        showSuggestions = false;
        if (OnChanged.HasDelegate)
        {
            await OnChanged.InvokeAsync();
        }
    }

    private async Task OnBlurAsync()
    {
        blurCts?.Cancel();
        blurCts = new CancellationTokenSource();
        var token = blurCts.Token;
        try
        {
            await Task.Delay(150, token);
            showSuggestions = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task OnFocusAsync()
    {
        blurCts?.Cancel();
        showSuggestions = true;
        await RefreshSuggestionsAsync();
    }

    private async Task OnInputChangedAsync()
    {
        showSuggestions = true;
        await RefreshSuggestionsAsync();
    }

    private async Task OnKeyUpAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && !string.IsNullOrWhiteSpace(input))
        {
            await CommitAsync(input.Trim());
            return;
        }

        if (args.Key == "Escape")
        {
            showSuggestions = false;
        }
    }

    private async Task RefreshSuggestionsAsync()
    {
        searchCts?.Cancel();
        searchCts = new CancellationTokenSource();
        var token = searchCts.Token;
        try
        {
            await Task.Delay(150, token);
            var results = await SearchAsync(input ?? string.Empty);
            if (token.IsCancellationRequested)
            {
                return;
            }

            var selectedNames = Selected.Select(NameOf).ToHashSet(StringComparer.OrdinalIgnoreCase);
            suggestions = results.Where(item => !selectedNames.Contains(NameOf(item))).Take(10).ToList();
            await InvokeAsync(StateHasChanged);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task RemoveAsync(TItem item)
    {
        Selected.Remove(item);
        if (OnChanged.HasDelegate)
        {
            await OnChanged.InvokeAsync();
        }
    }
}