// Small, flat surface for Blazor JS interop. Each function is single-purpose so the C# call
// sites read naturally.
window.netForge = {
    _themeModalDismissRegistered: new Set(),

    /**
     * Live-swaps the active Bootswatch stylesheet without a page reload.
     * @param {string} href Path under wwwroot to the theme's bootstrap.min.css.
     * @param {string} [dataBsTheme] "dark" or "light". Must match the theme, or Bootstrap's
     *   [data-bs-theme=dark] variable overrides won't apply and text goes unreadable.
     */
    setThemeHref: function (href, dataBsTheme) {
        var link = document.getElementById('theme-stylesheet');
        if (link) {
            link.setAttribute('href', href);
        }
        if (dataBsTheme === 'dark' || dataBsTheme === 'light') {
            document.documentElement.setAttribute('data-bs-theme', dataBsTheme);
        }
    },

    /** Raw value from localStorage; may be empty or invalid, so validate in .NET. */
    getStoredTheme: function (key) {
        try {
            return localStorage.getItem(key) || '';
        } catch (e) {
            return '';
        }
    },

    setStoredTheme: function (key, theme) {
        try {
            localStorage.setItem(key, theme);
        } catch (e) {
            /* private mode or blocked storage */
        }
    },

    /** Wires Bootstrap's hidden.bs.modal so Blazor can revert an uncommitted theme preview. */
    registerThemeModalDismiss: function (modalId, dotNetRef) {
        if (window.netForge._themeModalDismissRegistered.has(modalId)) {
            return;
        }
        var el = document.getElementById(modalId);
        if (!el) {
            return;
        }
        el.addEventListener('hidden.bs.modal', function () {
            dotNetRef.invokeMethodAsync('OnThemeModalDismissed');
        });
        window.netForge._themeModalDismissRegistered.add(modalId);
    },

    hideModal: function (modalId) {
        var el = document.getElementById(modalId);
        if (!el || typeof bootstrap === 'undefined') {
            return;
        }
        var instance = bootstrap.Modal.getInstance(el) || bootstrap.Modal.getOrCreateInstance(el);
        instance.hide();
    },

    /**
     * Selects everything before the extension so renaming "photo.jpg" doesn't make the user
     * retype ".jpg" or risk losing it.
     */
    selectFileNameStem: function (element) {
        if (!element) {
            return;
        }
        element.focus();
        var value = element.value || '';
        var dot = value.lastIndexOf('.');
        element.setSelectionRange(0, dot > 0 ? dot : value.length);
    }
};
