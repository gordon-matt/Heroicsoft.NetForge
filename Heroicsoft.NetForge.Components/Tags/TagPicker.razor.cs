using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Tags;

/// <summary>
/// Selected values as chips, plus a text field that suggests matches and can commit a new name.
/// The parent resolves that name into a real item and adds it to <c>Selected</c>.
/// </summary>
/// <typeparam name="TItem">The reference type shown on a chip and returned by search.</typeparam>
public partial class TagPicker<TItem> : IDisposable
    where TItem : class
{
    private CancellationTokenSource? blurCts;
    private string? input;
    private CancellationTokenSource? searchCts;
    private bool showSuggestions;
    private IReadOnlyList<TItem> suggestions = [];

    /// <summary>When false, typed text can only pick an existing suggestion.</summary>
    [Parameter]
    public bool AllowCreate { get; set; } = true;

    /// <summary>
    /// Extra classes on each selected chip. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? ChipClass { get; set; }

    /// <summary>
    /// Extra classes on the picker. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// <c>id</c> of the text field. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Word used in the placeholder and the empty message, lowercased. Required. Defaults to <c>Item</c>, so the empty state reads <c>No item yet.</c>
    /// </summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = "Item";

    /// <summary>Display name for an item. Required because the picker is generic.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> NameOf { get; set; } = _ => string.Empty;

    /// <summary>
    /// Called with the chosen name. The parent adds the resolved item to <c>Selected</c>.
    /// A blank name, or a name already selected ignoring case, clears the field and does not call this.
    /// Enter, a suggestion, and Create pass the trimmed name.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<string, Task> OnAddAsync { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Raised with no argument after a name is added or a chip is removed. Does not fire when the typed name is blank or already selected.
    /// </summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }

    /// <summary>Suggestions for the current text. Called as the user types, after a short delay, and again when the field is focused. Already selected names are hidden, and at most 10 suggestions are shown.</summary>
    [Parameter, EditorRequired]
    public Func<string, Task<IReadOnlyList<TItem>>> SearchAsync { get; set; } =
        _ => Task.FromResult<IReadOnlyList<TItem>>([]);

    /// <summary>
    /// Chips currently shown. Required. Removing a chip removes the item from this list. Adding is done in <c>OnAddAsync</c>. Defaults to an empty list.
    /// </summary>
    [Parameter, EditorRequired]
    public IList<TItem> Selected { get; set; } = [];

    private string Placeholder => $"Add {Label.ToLowerInvariant()}…";

    /// <summary>Cancels a pending suggestion search and the delayed hide of the suggestion list.</summary>
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