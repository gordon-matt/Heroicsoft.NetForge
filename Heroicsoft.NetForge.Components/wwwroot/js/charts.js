// Chart.js interop for the Chart component. Load chart.umd.js first, then this file, after
// nf.js when that file is on the page (it owns window.netForge). Formatting callbacks cannot
// survive JSON, so C# sets plugins.tooltip.nfFormat and they are attached here.
(function () {
    var forge = window.netForge || (window.netForge = {});
    var charts = new Map();

    function styleOf(name, fallback) {
        var root = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        if (root) {
            return root;
        }

        var body = getComputedStyle(document.body).getPropertyValue(name).trim();
        return body || fallback;
    }

    function applyThemeColors(config) {
        if (typeof Chart === 'undefined') {
            return config;
        }

        // Bootstrap hosts publish --bs-*; the nf shell publishes --nf-*. Prefer whichever is
        // actually painted, so a Bootswatch swap and an nf theme toggle both restyle the chart.
        var text = styleOf('--bs-secondary-color', '') || styleOf('--nf-muted', '#6b7380');
        var border = styleOf('--bs-border-color', '') || styleOf('--nf-border', 'rgba(128,128,128,0.25)');
        var font = styleOf('--bs-body-font-family', '') || styleOf('--nf-font', '');

        Chart.defaults.color = text;
        Chart.defaults.borderColor = border;

        if (font) {
            Chart.defaults.font.family = font;
        }

        return config;
    }

    function formatInfo(config) {
        var tooltip = config.options && config.options.plugins && config.options.plugins.tooltip;
        var format = (tooltip && tooltip.nfFormat) || (config.options && config.options.nfFormat) || '';
        var currencyCode = (tooltip && tooltip.nfCurrency) || (config.options && config.options.nfCurrency) || 'USD';

        return {
            format: format,
            currencyCode: currencyCode,
            percentage: !!(tooltip && tooltip.nfPercentage)
        };
    }

    function numberFormat(info, compact) {
        if (info.format === 'currency') {
            return new Intl.NumberFormat(undefined, {
                style: 'currency',
                currency: info.currencyCode,
                maximumFractionDigits: compact ? 0 : 2,
                notation: compact ? 'compact' : 'standard'
            });
        }

        if (info.format === 'percent') {
            return new Intl.NumberFormat(undefined, {
                style: 'percent',
                maximumFractionDigits: compact ? 0 : 1
            });
        }

        return new Intl.NumberFormat(undefined, {
            maximumFractionDigits: info.format === 'integer' || compact ? 0 : 2,
            notation: compact ? 'compact' : 'standard'
        });
    }

    function parsedValue(ctx) {
        var parsed = ctx.parsed;

        if (parsed && typeof parsed === 'object') {
            if (parsed.y !== undefined && parsed.y !== null) {
                return parsed.y;
            }

            if (parsed.x !== undefined && parsed.x !== null) {
                return parsed.x;
            }
        }

        return parsed;
    }

    function applyFormatters(config) {
        var info = formatInfo(config);

        if (!info.format || !config.options) {
            return config;
        }

        config.options.plugins = config.options.plugins || {};
        config.options.plugins.tooltip = config.options.plugins.tooltip || {};

        var tooltip = config.options.plugins.tooltip;
        var exact = numberFormat(info, false);
        var compact = numberFormat(info, true);

        tooltip.callbacks = {
            label: function (ctx) {
                var value = parsedValue(ctx);
                var formatted = info.format === 'percent'
                    ? exact.format((Number(value) || 0) / 100)
                    : exact.format(value);

                if (info.percentage) {
                    var total = ctx.dataset.data.reduce(function (sum, n) {
                        return sum + Math.abs(Number(n) || 0);
                    }, 0);

                    if (total > 0) {
                        var prefix = ctx.label ? ctx.label + ': ' : '';
                        return prefix + formatted + ' (' + (Math.abs(value) / total * 100).toFixed(1) + '%)';
                    }
                }

                return (ctx.dataset.label ? ctx.dataset.label + ': ' : '') + formatted;
            }
        };

        var valueAxis = config.options.indexAxis === 'y' ? 'x' : 'y';
        var scale = config.options.scales && config.options.scales[valueAxis];

        if (scale) {
            scale.ticks = Object.assign({}, scale.ticks, {
                callback: function (value) {
                    if (info.format === 'percent') {
                        return compact.format((Number(value) || 0) / 100);
                    }

                    return compact.format(value);
                }
            });
        }

        return config;
    }

    function restyle() {
        applyThemeColors({});
        charts.forEach(function (chart) {
            chart.update('none');
        });
    }

    forge.renderChart = function (canvasId, config) {
        var canvas = document.getElementById(canvasId);

        if (!canvas || typeof Chart === 'undefined') {
            return;
        }

        forge.destroyChart(canvasId);

        var chart = new Chart(canvas, applyFormatters(applyThemeColors(config)));
        charts.set(canvasId, chart);
    };

    forge.destroyChart = function (canvasId) {
        var existing = charts.get(canvasId);

        if (existing) {
            existing.destroy();
            charts.delete(canvasId);
        }
    };

    if (!forge._chartsListening) {
        forge._chartsListening = true;
        document.addEventListener('nf:themechange', restyle);
        document.addEventListener('netforge:themechange', function () {
            restyle();

            var link = document.getElementById('theme-stylesheet');

            if (link) {
                link.addEventListener('load', restyle, { once: true });
            }
        });
    }
})();
