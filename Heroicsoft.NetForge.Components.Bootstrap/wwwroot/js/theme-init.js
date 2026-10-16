/**
 * Runs in <head> before paint so the stored theme applies without a flash of the default.
 * The script tag supplies data-theme-key and data-default-theme.
 * ALLOWED and DARK stay in lock-step with BootswatchThemes.
 */
(function () {
    var script = document.currentScript;
    var KEY = (script && script.dataset.themeKey) || 'netforge.theme';
    var ALLOWED = [
        'brite', 'cerulean', 'cosmo', 'cyborg', 'darkly', 'flatly', 'journal', 'litera', 'lumen',
        'lux', 'materia', 'minty', 'morph', 'pulse', 'quartz', 'sandstone', 'simplex', 'sketchy',
        'slate', 'solar', 'spacelab', 'superhero', 'united', 'vapor', 'yeti', 'zephyr'
    ];
    var DARK = { cyborg: 1, darkly: 1, slate: 1, solar: 1, superhero: 1, vapor: 1 };

    try {
        var raw = localStorage.getItem(KEY);
        if (!raw) return;

        var theme = String(raw).toLowerCase();
        if (ALLOWED.indexOf(theme) < 0) return;

        var format = (script && script.dataset.stylesheetFormat) || 'lib/bootswatch/dist/{0}/bootstrap.min.css';
        var link = document.getElementById('theme-stylesheet');
        if (link) {
            link.setAttribute('href', format.replace('{0}', theme));
        }

        document.documentElement.setAttribute('data-bs-theme', DARK[theme] ? 'dark' : 'light');
    } catch (e) {
        /* private mode or blocked storage */
    }
})();
