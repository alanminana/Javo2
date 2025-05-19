// debug.js - Módulo para depuración de AJAX y diagnóstico
(function (window, $) {
    'use strict';

    // Asegurar que App.config.debug sea siempre función antes de que core-bundle lo invoque
    window.App = window.App || {};
    window.App.config = window.App.config || {};
    if (typeof window.App.config.debug !== 'function') {
        window.App.config.debug = function (message, data) {
            // Stub: no hace nada hasta inicializar el verdadero debug
        };
    }

    var App = window.App;

    App.debug = {
        enabled: false,

        /**
         * Inicializa el módulo de debug usando el flag App.config.debugMode
         * y refuerza App.config.debug como función válida.
         */
        init: function () {
            // Obtiene el flag definido en _Layout.cshtml
            var flag = App.config && App.config.debugMode;
            this.enabled = (flag === true || flag === 'true');

            if (this.enabled) {
                this.setupAjaxLogging();
                this.logAntiForgeryToken();
                console.log('Módulo de depuración inicializado');
            }

            // Refuerza que App.config.debug use nuestro logger interno
            App.config.debug = function (message, data) {
                App.debug.log(message, data);
            };
        },

        // Registrar solicitudes AJAX
        setupAjaxLogging: function () {
            $(document).ajaxSend(function (event, jqXHR, settings) {
                console.log('AJAX Request:', {
                    url: settings.url,
                    type: settings.type,
                    data: settings.data
                });
            });

            $(document).ajaxComplete(function (event, jqXHR, settings) {
                console.log('AJAX Response:', {
                    status: jqXHR.status,
                    statusText: jqXHR.statusText,
                    responseText: jqXHR.responseText
                });
            });
        },

        // Verificar si el token anti-falsificación está presente
        logAntiForgeryToken: function () {
            $(function () {
                var token = $('input[name="__RequestVerificationToken"]').val();
                console.log('Anti-forgery token encontrado:', token ? token : 'No encontrado');
            });
        },

        // Registrar mensaje de depuración con datos
        log: function (message, data) {
            if (this.enabled) {
                if (data !== undefined) {
                    console.log('[DEBUG] ' + message, data);
                } else {
                    console.log('[DEBUG] ' + message);
                }
            }
        },

        // Registrar mensaje de error
        error: function (message, err) {
            if (this.enabled) {
                console.error('[ERROR] ' + message, err || '');
            }
        }
    };

    // Arranca el módulo de debug al cargar el DOM
    $(function () {
        if (App.debug && typeof App.debug.init === 'function') {
            App.debug.init();
        }
    });

})(window, jQuery);
