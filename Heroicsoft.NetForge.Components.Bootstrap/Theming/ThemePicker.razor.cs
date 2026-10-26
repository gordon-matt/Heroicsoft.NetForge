using Heroicsoft.NetForge.Abstractions.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Bootstrap.Theming;

public partial class ThemePicker : IAsyncDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IOptions<ForgeBootstrapOptions> UiOptions { get; set; } = default!;

    private const string ModalId = "bf-theme-picker";

    private DotNetObjectReference<ThemePicker>? dotNetRef;
    private string activeKey = "";
    private string committedTheme = BootswatchThemes.Default;
    private string previewTheme = BootswatchThemes.Default;
    private bool closedWithOk;
    private bool showDarkTab = BootswatchThemes.IsDarkColorScheme(BootswatchThemes.Default);

    private ForgeBootstrapOptions Ui => UiOptions.Value;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        dotNetRef = DotNetObjectReference.Create(this);

        // Dismissing with Escape or the backdrop has to revert the preview, and Bootstrap only
        // reports that through its own event.
        await JS.InvokeVoidAsync("netForge.registerThemeModalDismiss", ModalId, dotNetRef);
        await LoadStoredThemeAsync();
        StateHasChanged();
    }

    private async Task LoadStoredThemeAsync()
    {
        activeKey = await StorageKeyAsync();
        string area = await JS.InvokeAsync<string>("netForge.currentThemeArea");
        string configured = ThemeStorage.ValueFor(area, Ui.Areas, Ui.DefaultTheme);
        if (!BootswatchThemes.TryNormalize(configured, out string fallback, BootswatchThemes.Default))
        {
            fallback = BootswatchThemes.Default;
        }

        string raw = await JS.InvokeAsync<string>("netForge.getStoredTheme", activeKey);
        BootswatchThemes.TryNormalize(raw, out string stored, fallback);
        committedTheme = stored;
        previewTheme = stored;
        showDarkTab = BootswatchThemes.IsDarkColorScheme(stored);
    }

    private async Task<string> StorageKeyAsync()
    {
        string live = await JS.InvokeAsync<string>("netForge.currentStorageKey");
        return string.IsNullOrEmpty(live) ? ThemeStorage.KeyFor(Ui.ThemeStorageKey, null) : live;
    }

    private async Task PreviewThemeAsync(string theme)
    {
        if (string.Equals(theme, previewTheme, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ApplyThemeToDomAsync(theme);
        StateHasChanged();
    }

    private async Task OkAsync()
    {
        await JS.InvokeVoidAsync("netForge.setStoredTheme", activeKey, previewTheme);
        committedTheme = previewTheme;
        closedWithOk = true;
        await JS.InvokeVoidAsync("netForge.hideModal", ModalId);
    }

    private async Task CancelAsync()
    {
        await ApplyThemeToDomAsync(committedTheme);
        await JS.InvokeVoidAsync("netForge.hideModal", ModalId);
    }

    private async Task ApplyThemeToDomAsync(string theme)
    {
        bool isDark = BootswatchThemes.IsDarkColorScheme(theme);

        await JS.InvokeVoidAsync(
            "netForge.setThemeHref",
            BootswatchThemes.StylesheetHref(theme, Ui.StylesheetFormat),
            isDark ? "dark" : "light");

        previewTheme = theme;
        showDarkTab = isDark;
    }

    /// <summary>Reloads when navigation has moved the page into another area.</summary>
    [JSInvokable]
    public async Task OnThemeModalOpening()
    {
        string live = await JS.InvokeAsync<string>("netForge.currentStorageKey");
        if (string.IsNullOrEmpty(live) || string.Equals(live, activeKey, StringComparison.Ordinal))
        {
            return;
        }

        await LoadStoredThemeAsync();
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnThemeModalDismissed()
    {
        if (closedWithOk)
        {
            closedWithOk = false;
            return;
        }

        if (!string.Equals(previewTheme, committedTheme, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyThemeToDomAsync(committedTheme);
            await InvokeAsync(StateHasChanged);
        }
    }

    public ValueTask DisposeAsync()
    {
        dotNetRef?.Dispose();
        dotNetRef = null;
        return ValueTask.CompletedTask;
    }

    private static string TitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}