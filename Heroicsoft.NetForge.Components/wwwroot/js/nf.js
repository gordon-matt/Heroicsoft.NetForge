// Small, flat surface for the nf component set. Theme toggling is handled by a delegated click
// handler, so ThemeToggle works without a Blazor circuit (static SSR pages, error pages).
window.netForge = {
    /** The scheme currently applied to <html>: 'light' or 'dark'. */
    getTheme: function () {
        return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
    },

    /** The key theme-init.js is currently reading and writing. Empty before that script runs. */
    currentStorageKey: function () {
        return document.documentElement.getAttribute('data-nf-storage-key') || '';
    },

    /** The area whose theme is applied. Empty string is the default area (the public site). */
    currentThemeArea: function () {
        return document.documentElement.getAttribute('data-nf-theme-area') || '';
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

        document.dispatchEvent(new CustomEvent('nf:themechange', {
            detail: {
                theme: theme,
                area: document.documentElement.getAttribute('data-nf-theme-area') || '',
                storageKey: key || ''
            }
        }));
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
    },

    /**
     * Plays a toast sound, like egalink-toasty's playSound. Browsers may block audio until the user
     * has interacted with the page; that rejection is ignored.
     */
    /**
     * Pins an open ActionMenu list to its trigger with fixed positioning, so a table wrapper with
     * overflow does not clip it. Flips above the trigger when there is no room below, and follows
     * the trigger while the page scrolls until the list leaves the DOM.
     */
    placeMenu: function (trigger, menu) {
        if (!trigger || !menu) {
            return;
        }

        function place() {
            if (!menu.isConnected) {
                window.removeEventListener('scroll', place, true);
                window.removeEventListener('resize', place);
                return;
            }

            var bounds = trigger.getBoundingClientRect();
            var width = menu.offsetWidth;
            var height = menu.offsetHeight;
            var gap = 4;
            var top = bounds.bottom + gap;
            if (top + height > window.innerHeight && bounds.top - gap - height > 0) {
                top = bounds.top - gap - height;
            }

            var left = Math.max(8, Math.min(bounds.right - width, window.innerWidth - width - 8));
            menu.style.position = 'fixed';
            menu.style.top = top + 'px';
            menu.style.left = left + 'px';
            menu.style.right = 'auto';
        }

        place();
        window.addEventListener('scroll', place, true);
        window.addEventListener('resize', place);

        var first = menu.querySelector('[role="menuitem"]:not([disabled])');
        if (first) {
            first.focus();
        }
    },

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

// Interactive Server intercepts same-origin clicks while the event is bubbling and does not look
// at data-enhance-nav. A capture listener on window runs before that, including for a click whose
// target is text inside the link. Stopping it lets the browser navigate or download normally.
window.addEventListener('click', function (event) {
    if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
    }

    var link = null;
    var path = event.composedPath ? event.composedPath() : [];
    for (var i = 0; i < path.length; i++) {
        var node = path[i];
        if (node && node.tagName === 'A' && node.hasAttribute('href')) {
            link = node;
            break;
        }
    }

    if (!link && event.target && event.target.closest) {
        link = event.target.closest('a[href]');
    }

    if (!link) {
        return;
    }

    var optedOut = (link.getAttribute('data-enhance-nav') || '').toLowerCase() === 'false';
    if (!optedOut && !link.hasAttribute('download')) {
        return;
    }

    event.stopPropagation();
}, true);

document.addEventListener('click', function (event) {
    var toggle = event.target.closest && event.target.closest('[data-nf-theme-toggle]');
    if (toggle) {
        // Prefer the live key. Client-side navigation between areas updates it without
        // re-rendering this button.
        var key = document.documentElement.getAttribute('data-nf-storage-key') || toggle.getAttribute('data-theme-key');
        window.netForge.toggleTheme(key);
    }
});

// Top-bar menus follow a desktop menu bar. A click arms the bar and opens a dropdown. While armed,
// hovering another dropdown switches to it, and hovering a plain top-level item hides the panel
// without disarming, so the next dropdown still opens on hover. A click outside, Escape, or a click
// on the open item disarms the bar. Touch keeps click-to-toggle. The same script lives in forge.js;
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
