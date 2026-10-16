/**
 * Runs in <head> before first paint, so the stored scheme is applied without a flash of the
 * default. ForgeHostPage supplies the script tag's data attributes:
 *   data-theme-key      localStorage key (ForgeUiOptions.ThemeStorageKey)
 *   data-default-theme  light | dark | system, used when nothing is stored yet
 */
(function () {
    var script = document.currentScript;
    var key = (script && script.dataset.themeKey) || 'netforge.theme';
    var fallback = (script && script.dataset.defaultTheme) || 'light';
    var theme = null;

    try {
        theme = localStorage.getItem(key);
    } catch (e) {
        /* private mode or blocked storage */
    }

    if (theme !== 'light' && theme !== 'dark') {
        if (fallback === 'system') {
            theme = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        } else {
            theme = fallback === 'dark' ? 'dark' : 'light';
        }
    }

    document.documentElement.setAttribute('data-theme', theme);
})();
