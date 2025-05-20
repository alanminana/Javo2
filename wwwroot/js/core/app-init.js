// app-init.js - Inicialización centralizada de la aplicación
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    // Configuración global
    App.config = {
        debug: false,
        apiBaseUrl: '',
        currency: 'ARS',
        dateFormat: 'DD/MM/YYYY',
        enableAnimations: true,
        version: '1.0.0'
    };

    // Inicialización principal
    App.init = function (options) {
        // Combinar opciones
        this.config = $.extend({}, this.config, options);

        // Configurar módulo de debug
        this._setupDebug();

        // Inicializar móldulos principales
        this._initCoreModules();

        // Configurar manejadores globales
        this._setupGlobalHandlers();

        // Inicializar controladores según la página
        this._initPageControllers();

        if (this.config.debug) {
            console.log('App inicializada correctamente');
        }
    };

    // Configurar módulo de debug
    App._setupDebug = function () {
        // Garantizar que debug siempre es una función
        var originalDebug = this.debug;
        this.debug = function (message, data) {
            if (this.config.debug) {
                console.log(message, data || '');
            }
        };

        // Añadir métodos auxiliares
        this.debug.log = this.debug;
        this.debug.error = function (message, data) {
            console.error(message, data || '');
        };
        this.debug.warn = function (message, data) {
            console.warn(message, data || '');
        };
        this.debug.info = function (message, data) {
            console.info(message, data || '');
        };
    };

    // Inicializar módulos principales
    App._initCoreModules = function () {
        // Inicializar utilidades primero
        if (App.format) App.debug.log('Inicializando formato');
        if (App.notify) {
            App.debug.log('Inicializando notificaciones');
            App.notify.init();
        }
        if (App.ajax) App.debug.log('Inicializando AJAX');
        if (App.validation) App.debug.log('Inicializando validación');

        // Inicializar componentes base
        if (App.tables) {
            App.debug.log('Inicializando tablas');
            App.tables.init();
        }
        if (App.permissions) {
            App.debug.log('Inicializando permisos');
            App.permissions.init();
        }
    };

    // Configurar manejadores globales
    App._setupGlobalHandlers = function () {
        // Confirmación de eliminación
        $(document).on('click', 'a[href*="Delete"], .btn-danger[href*="Delete"]', function (e) {
            if (!confirm('¿Está seguro que desea eliminar este elemento? Esta acción no se puede deshacer.')) {
                e.preventDefault();
                return false;
            }
            return true;
        });

        // Prevenir doble envío de formularios
        $(document).on('submit', 'form', function () {
            var $form = $(this);
            var $submitButton = $form.find('button[type="submit"]');

            if ($form.data('submitting')) {
                return false;
            }

            if ($submitButton.length) {
                var originalText = $submitButton.html();
                $submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Procesando...');
                $form.data('submitting', true).data('original-text', originalText);

                setTimeout(function () {
                    $form.data('submitting', false);
                    $submitButton.prop('disabled', false).html(originalText);
                }, 10000); // Timeout de seguridad de 10 segundos
            }
        });
    };

    // Inicializar controladores según la página actual
    App._initPageControllers = function () {
        // Detectar controlador activo basado en clases o atributos del body
        const bodyClasses = document.body.className.split(' ');
        const pageController = document.body.getAttribute('data-controller');
        const pageAction = document.body.getAttribute('data-action');

        // Si hay un controlador explícito definido
        if (pageController) {
            const controllerName = this._toCamelCase(pageController);
            if (App[controllerName]) {
                App.debug.log(`Inicializando controlador: ${controllerName}`);
                App[controllerName].init();

                // Si hay una acción específica
                if (pageAction && App[controllerName][pageAction]) {
                    App.debug.log(`Ejecutando acción: ${pageAction}`);
                    App[controllerName][pageAction]();
                }
            }
        }

        // Inicializar controladores basados en clases del body
        bodyClasses.forEach(className => {
            if (className.endsWith('-page')) {
                const controllerName = this._toCamelCase(className.replace('-page', ''));
                if (App[controllerName] && typeof App[controllerName].init === 'function') {
                    App.debug.log(`Inicializando controlador por clase: ${controllerName}`);
                    App[controllerName].init();
                }
            }
        });
    };

    // Convertir string a camelCase
    App._toCamelCase = function (str) {
        return str.replace(/-([a-z])/g, function (g) {
            return g[1].toUpperCase();
        });
    };

    // Inicializar cuando el documento esté listo
    $(document).ready(function () {
        App.init();
    });

})(window, jQuery);