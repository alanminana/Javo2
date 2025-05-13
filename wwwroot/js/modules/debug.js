// debug.js - Módulo para depuración de AJAX y diagnóstico
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.debug = {
        enabled: false,

        init: function (config) {
            this.enabled = config && config.enabled;

            if (this.enabled) {
                this.setupAjaxLogging();
                this.logAntiForgeryToken();
                console.log('Módulo de depuración inicializado');
            }
        },

        // Registrar solicitudes AJAX
        setupAjaxLogging: function () {
            $(document).ajaxSend(function (event, jqXHR, settings) {
                console.log('AJAX Request:', {
                    url: settings.url,
                    type: settings.type,
                    data: settings.data,
                    headers: settings.headers
                });
            });

            $(document).ajaxComplete(function (event, jqXHR, settings) {
                console.log('AJAX Response:', {
                    status: jqXHR.status,
                    statusText: jqXHR.statusText,
                    responseText: jqXHR.responseText,
                    url: settings.url
                });
            });
        },

        // Verificar si el token anti-falsificación está presente
        logAntiForgeryToken: function () {
            $(document).ready(function () {
                const token = $('input[name="__RequestVerificationToken"]').val();
                console.log('Anti-forgery token encontrado:', token ? 'Sí' : 'No', token);
            });
        },

        // Registrar mensaje de depuración con datos
        log: function (message, data) {
            if (this.enabled) {
                if (data) {
                    console.log(`[DEBUG] ${message}`, data);
                } else {
                    console.log(`[DEBUG] ${message}`);
                }
            }
        },

        // Registrar mensaje de error
        error: function (message, error) {
            if (this.enabled) {
                console.error(`[ERROR] ${message}`, error || '');
            }
        }
    };

})(window, jQuery);