/**
 * Runs in <head> before first paint, so the stored scheme is applied without a flash of the
 * default. ForgeHostPage supplies the script tag's data attributes:
 *   data-theme-key      base localStorage key (ForgeUiOptions.ThemeStorageKey)
 *   data-default-theme  light | dark | system, used when nothing is stored for the area
 *   data-theme-areas    JSON of area name to default scheme. Absent when the app has no named areas.
 *   data-theme-area     pins this document to one area. Omit it to follow the URL.
 * The key format matches ThemeStorage.KeyFor: the base key, then "." and the lower-case area.
 */
(function () {
    var script = document.currentScript;
    var baseKey = (script && script.dataset.themeKey) || 'netforge.theme';
    var fallback = (script && script.dataset.defaultTheme) || 'light';
    var areas = readAreas(script);
    var pinned = script && script.hasAttribute('data-theme-area');
    var current = resolveArea();

    apply(current, false);
    if (!pinned && hasAreas()) {
        watchNavigation();
    }

    function readAreas(source) {
        if (!source || !source.dataset.themeAreas) {
            return {};
        }
        try {
            return JSON.parse(source.dataset.themeAreas) || {};
        } catch (e) {
            return {};
        }
    }

    function hasAreas() {
        for (var name in areas) {
            if (Object.prototype.hasOwnProperty.call(areas, name)) {
                return true;
            }
        }
        return false;
    }

    function normalize(area) {
        return String(area || '').trim().toLowerCase();
    }

    function storageKey(area) {
        var key = String(baseKey || '').trim() || 'netforge.theme';
        var name = normalize(area);
        return name ? key + '.' + name : key;
    }

    function firstSegment() {
        var path = window.location.pathname || '/';
        var base = document.querySelector('base');
        if (base && base.getAttribute('href')) {
            try {
                var basePath = new URL(base.href, window.location.origin).pathname.replace(/\/$/, '');
                if (basePath.length > 1 && (path === basePath || path.indexOf(basePath + '/') === 0)) {
                    path = path.slice(basePath.length) || '/';
                }
            } catch (e) {
                /* the base href is not a URL; use the path as it stands */
            }
        }

        var parts = path.split('/');
        for (var i = 0; i < parts.length; i++) {
            if (parts[i]) {
                return normalize(parts[i]);
            }
        }
        return '';
    }

    function resolveArea() {
        if (pinned) {
            return normalize(script.getAttribute('data-theme-area'));
        }
        var segment = firstSegment();
        if (segment && Object.prototype.hasOwnProperty.call(areas, segment)) {
            return segment;
        }
        return '';
    }

    function fallbackFor(area) {
        var name = normalize(area);
        if (name && areas[name]) {
            return String(areas[name]).toLowerCase();
        }
        return fallback;
    }

    function read(key) {
        try {
            return localStorage.getItem(key);
        } catch (e) {
            return null;
        }
    }

    function resolveScheme(area) {
        var theme = read(storageKey(area));
        var areaFallback = fallbackFor(area);
        if (theme !== 'light' && theme !== 'dark') {
            if (areaFallback === 'system') {
                theme = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            } else {
                theme = areaFallback === 'dark' ? 'dark' : 'light';
            }
        }
        return theme;
    }

    function apply(area, notify) {
        var theme = resolveScheme(area);
        var root = document.documentElement;
        var previous = root.getAttribute('data-theme');
        root.setAttribute('data-theme', theme);
        root.setAttribute('data-nf-storage-key', storageKey(area));
        root.setAttribute('data-nf-theme-area', area || '');
        if (notify && previous !== theme) {
            document.dispatchEvent(new CustomEvent('nf:themechange', {
                detail: { theme: theme, area: area || '', storageKey: storageKey(area) }
            }));
        }
    }

    function watchNavigation() {
        var push = history.pushState;
        var replace = history.replaceState;
        history.pushState = function () {
            push.apply(history, arguments);
            onNavigate();
        };
        history.replaceState = function () {
            replace.apply(history, arguments);
            onNavigate();
        };
        window.addEventListener('popstate', onNavigate);
    }

    function onNavigate() {
        var next = resolveArea();
        if (next === current) {
            return;
        }
        current = next;
        apply(current, true);
    }
})();
