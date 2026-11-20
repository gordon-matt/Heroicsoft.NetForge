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
            // Same attribute the nf shell uses, so a library stylesheet can follow either host.
            document.documentElement.setAttribute('data-theme', dataBsTheme);
        }
        document.dispatchEvent(new CustomEvent('netforge:themechange', {
            detail: {
                href: href,
                colorScheme: dataBsTheme,
                area: document.documentElement.getAttribute('data-nf-theme-area') || '',
                storageKey: document.documentElement.getAttribute('data-nf-storage-key') || ''
            }
        }));
    },

    /** The key theme-init.js is currently reading and writing. Empty before that script runs. */
    currentStorageKey: function () {
        return document.documentElement.getAttribute('data-nf-storage-key') || '';
    },

    /** The area whose theme is applied. Empty string is the default area (the public site). */
    currentThemeArea: function () {
        return document.documentElement.getAttribute('data-nf-theme-area') || '';
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
        el.addEventListener('show.bs.modal', function () {
            dotNetRef.invokeMethodAsync('OnThemeModalOpening');
        });
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
    },

    /**
     * Plays a toast sound, like egalink-toasty's playSound. Browsers may block audio until the user
     * has interacted with the page; that rejection is ignored.
     */
    playSound: function (src) {
        if (!src) {
            return;
        }
        try {
            var audio = new Audio(src);
            var played = audio.play();
            if (played && played.catch) {
                played.catch(function () { /* autoplay blocked */ });
            }
        } catch (e) {
            /* audio not supported */
        }
    }
};

// Top-bar menus follow a desktop menu bar. A click arms the bar and opens a dropdown. While armed,
// hovering another dropdown switches to it, and hovering a plain top-level item hides the panel
// without disarming, so the next dropdown still opens on hover. A click outside, Escape, or a click
// on the open item disarms the bar. Touch keeps click-to-toggle. The same script lives in nf.js;
// the flag keeps a page that loads both from handling each pointer twice.
if (!window.__nfMenuBar) {
    window.__nfMenuBar = true;

    function menuBarOf(node) {
        return node && node.closest ? node.closest('.nf-shared-topnav, .bf-shared-topnav') : null;
    }

    function topItem(bar, node) {
        var el = node;
        while (el && el !== bar) {
            if (el.parentElement === bar) {
                return el;
            }
            el = el.parentElement;
        }
        return null;
    }

    function itemDetails(item) {
        var child = item ? item.firstElementChild : null;
        while (child) {
            if (child.tagName === 'DETAILS') {
                return child;
            }
            child = child.nextElementSibling;
        }
        return null;
    }

    function barIsOpen(bar) {
        var items = bar.children;
        for (var i = 0; i < items.length; i++) {
            var details = itemDetails(items[i]);
            if (details && details.open) {
                return true;
            }
        }
        return false;
    }

    function setExpanded(details) {
        var summary = details.querySelector('summary');
        if (summary && summary.hasAttribute('aria-expanded')) {
            summary.setAttribute('aria-expanded', details.open ? 'true' : 'false');
        }
    }

    function closeOtherMenus(bar, except) {
        var items = bar.children;
        for (var i = 0; i < items.length; i++) {
            var details = itemDetails(items[i]);
            if (details && details !== except && details.open) {
                details.open = false;
            }
        }
    }

    function isArmed(bar) {
        return bar.getAttribute('data-nf-menu-active') === 'true' || barIsOpen(bar);
    }

    // Set while a hover hides panels, so that close does not also disarm the bar.
    var holding = false;

    function arm(bar) {
        bar.setAttribute('data-nf-menu-active', 'true');
    }

    function disarm(bar) {
        holding = true;
        closeOtherMenus(bar, null);
        holding = false;
        bar.removeAttribute('data-nf-menu-active');
    }

    function eachBar(visit) {
        var bars = document.querySelectorAll('.nf-shared-topnav, .bf-shared-topnav');
        for (var i = 0; i < bars.length; i++) {
            visit(bars[i]);
        }
    }

    document.addEventListener('pointerover', function (event) {
        if (event.pointerType !== 'mouse') {
            return;
        }

        var bar = menuBarOf(event.target);
        if (!bar || !isArmed(bar)) {
            return;
        }

        var item = topItem(bar, event.target);
        if (!item) {
            return;
        }

        var from = event.relatedTarget;
        if (from && item.contains(from)) {
            return;
        }

        var details = itemDetails(item);
        if (details && details.open) {
            return;
        }

        holding = true;
        closeOtherMenus(bar, details);
        holding = false;
        if (details) {
            details.open = true;
        } else {
            arm(bar);
        }
    });

    document.addEventListener('toggle', function (event) {
        var details = event.target;
        if (!details || details.tagName !== 'DETAILS') {
            return;
        }

        setExpanded(details);

        var bar = menuBarOf(details);
        if (!bar) {
            return;
        }

        var item = topItem(bar, details);
        if (!item || itemDetails(item) !== details) {
            return;
        }

        if (details.open) {
            arm(bar);
            closeOtherMenus(bar, details);
            return;
        }

        // A click on the open item closes it and disarms. A hover onto a plain item does not.
        if (!holding && !barIsOpen(bar)) {
            bar.removeAttribute('data-nf-menu-active');
        }
    }, true);

    document.addEventListener('click', function (event) {
        eachBar(function (bar) {
            if (bar.contains(event.target)) {
                var link = event.target.closest && event.target.closest('a[href]');
                if (link && bar.contains(link)) {
                    disarm(bar);
                }
                return;
            }

            if (isArmed(bar)) {
                disarm(bar);
            }
        });
    });

    document.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape') {
            return;
        }

        eachBar(function (bar) {
            if (isArmed(bar)) {
                disarm(bar);
            }
        });
    });
}
