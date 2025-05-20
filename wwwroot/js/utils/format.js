// format.js - Utilidades de formateo
(function (window) {
    'use strict';

    var App = window.App = window.App || {};

    App.format = {
        // Formatear moneda
        currency: function (value, options) {
            options = options || {};
            const currency = options.currency || App.config.currency || 'ARS';
            const locale = options.locale || 'es-AR';

            return new Intl.NumberFormat(locale, {
                style: 'currency',
                currency: currency,
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).format(value || 0);
        },

        // Formatear fecha
        date: function (date, options) {
            options = options || {};
            const format = options.format || App.config.dateFormat || 'DD/MM/YYYY';
            const locale = options.locale || 'es-AR';

            if (!date) return '';

            const d = new Date(date);
            if (isNaN(d.getTime())) return '';

            if (format === 'locale') {
                return d.toLocaleDateString(locale);
            }

            // Formato personalizado
            const day = d.getDate().toString().padStart(2, '0');
            const month = (d.getMonth() + 1).toString().padStart(2, '0');
            const year = d.getFullYear();

            return format
                .replace('DD', day)
                .replace('MM', month)
                .replace('YYYY', year);
        },

        // Formatear número
        number: function (value, decimals) {
            decimals = decimals !== undefined ? decimals : 2;
            return new Intl.NumberFormat('es-AR', {
                minimumFractionDigits: decimals,
                maximumFractionDigits: decimals
            }).format(value || 0);
        },

        // Formatear porcentaje
        percent: function (value, decimals) {
            decimals = decimals !== undefined ? decimals : 2;
            return new Intl.NumberFormat('es-AR', {
                style: 'percent',
                minimumFractionDigits: decimals,
                maximumFractionDigits: decimals
            }).format((value || 0) / 100);
        }
    };

})(window);