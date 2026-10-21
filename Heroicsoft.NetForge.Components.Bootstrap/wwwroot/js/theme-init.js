/**
 * Runs in <head> before paint so the stored theme applies without a flash of the default.
 * The script tag supplies data-theme-key, data-default-theme, and optionally data-theme-areas
 * and data-theme-area. The key format matches ThemeStorage.KeyFor.
 * ALLOWED and DARK stay in lock-step with BootswatchThemes.
 */
(function () {
    var script = document.currentScript;
    var baseKey = (script && script.dataset.themeKey) || 'netforge.theme';
    var fallback = (script && script.dataset.defaultTheme) || '';
    var format = (script && script.dataset.stylesheetFormat) || 'lib/bootswatch/dist/{0}/bootstrap.min.css';
    var areas = readAreas(script);
    var pinned = script && script.hasAttribute('data-theme-area');
    var ALLOWED = [
        'brite', 'cerulean', 'cosmo', 'cyborg', 'darkly', 'flatly', 'journal', 'litera', 'lumen',
        'lux', 'materia', 'minty', 'morph', 'pulse', 'quartz', 'sandstone', 'simplex', 'sketchy',
        'slate', 'solar', 'spacelab', 'superhero', 'united', 'vapor', 'yeti', 'zephyr'
    ];
    var DARK = { cyborg: 1, darkly: 1, slate: 1, solar: 1, superhero: 1, vapor: 1 };
    var current = resolveArea();

    try {
        apply(current, false);
        if (!pinned && hasAreas()) {
            watchNavigation();
        }
    } catch (e) {
        /* private mode or blocked storage */
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

    function fallbackTheme(area) {
        var name = normalize(area);
        var candidate = name && areas[name] ? String(areas[name]) : fallback;
        candidate = String(candidate || '').trim().toLowerCase();
        return ALLOWED.indexOf(candidate) >= 0 ? candidate : '';
    }

    function read(key) {
        try {
            return localStorage.getItem(key);
        } catch (e) {
            return null;
        }
    }

    function apply(area, notify) {
        var root = document.documentElement;
        var key = storageKey(area);
        root.setAttribute('data-nf-storage-key', key);
        root.setAttribute('data-nf-theme-area', area || '');

        var raw = read(key);
        var stored = raw ? String(raw).toLowerCase() : '';
        var hasStored = ALLOWED.indexOf(stored) >= 0;
        var theme = hasStored ? stored : fallbackTheme(area);
        if (ALLOWED.indexOf(theme) < 0) {
            return;
        }

        var scheme = DARK[theme] ? 'dark' : 'light';
        var previousScheme = root.getAttribute('data-bs-theme');
        var link = document.getElementById('theme-stylesheet');
        var previousHref = link ? link.getAttribute('href') : '';
        var href = format.replace('{0}', theme);

        root.setAttribute('data-bs-theme', scheme);
        root.setAttribute('data-theme', scheme);

        // Nothing stored yet: the server already rendered this area's default stylesheet.
        if (!hasStored && !notify) {
            return;
        }

        if (link && previousHref !== href) {
            link.setAttribute('href', href);
        }

        if (notify && (previousHref !== href || previousScheme !== scheme)) {
            document.dispatchEvent(new CustomEvent('netforge:themechange', {
                detail: { href: href, colorScheme: scheme, area: area || '', storageKey: key }
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
