using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Bootstrap.Tags;

/// <summary>
/// Selected values as chips, plus a text field that suggests matches and can commit a new name.
/// The parent resolves that name into a real item and adds it to <see cref="Selected"/>.
/// </summary>
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

    [Parameter]
    public string? ChipClass { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter, EditorRequired]
    public string Label { get; set; } = "Item";

    /// <summary>Display name for an item. Required because the picker is generic.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> NameOf { get; set; } = _ => string.Empty;

    /// <summary>
    /// Called with the chosen name. The parent adds the resolved item to <see cref="Selected"/>.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<string, Task> OnAddAsync { get; set; } = _ => Task.CompletedTask;

    [Parameter]
    public EventCallback OnChanged { get; set; }

    /// <summary>Suggestions for the current text. Called as the user types.</summary>
    [Parameter, EditorRequired]
    public Func<string, Task<IReadOnlyList<TItem>>> SearchAsync { get; set; } =
        _ => Task.FromResult<IReadOnlyList<TItem>>([]);

    [Parameter, EditorRequired]
    public IList<TItem> Selected { get; set; } = [];

    private string Placeholder => $"Add {Label.ToLowerInvariant()}…";

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
