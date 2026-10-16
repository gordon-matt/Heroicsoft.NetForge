// Small, flat surface for the nf component set. Theme toggling is handled by a delegated click
// handler, so ThemeToggle works without a Blazor circuit (static SSR pages, error pages).
window.netForge = {
    /** The scheme currently applied to <html>: 'light' or 'dark'. */
    getTheme: function () {
        return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
    },

    /**
     * Applies and stores a scheme. Raises 'nf:themechange' on document so an app can react, for
     * example by swapping a syntax-highlighting stylesheet.
     */
    setTheme: function (key, theme) {
        theme = theme === 'dark' ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', theme);

        if (key) {
            try {
                localStorage.setItem(key, theme);
            } catch (e) {
                /* private mode or blocked storage */
            }
        }

        document.dispatchEvent(new CustomEvent('nf:themechange', { detail: { theme: theme } }));
    },

    toggleTheme: function (key) {
        window.netForge.setTheme(key, window.netForge.getTheme() === 'dark' ? 'light' : 'dark');
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

document.addEventListener('click', function (event) {
    var toggle = event.target.closest && event.target.closest('[data-nf-theme-toggle]');
    if (toggle) {
        window.netForge.toggleTheme(toggle.getAttribute('data-theme-key'));
    }
});

// "/" focuses the topbar search, unless the user is already typing somewhere.
document.addEventListener('keydown', function (event) {
    if (event.key !== '/' || event.ctrlKey || event.metaKey || event.altKey) {
        return;
    }

    var active = document.activeElement;
    var typing = active && (active.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(active.tagName));
    var search = document.querySelector('[data-nf-search]');

    if (!typing && search) {
        event.preventDefault();
        search.focus();
        search.select();
    }
});
