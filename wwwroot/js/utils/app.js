// app.js - Aplicación principal
(function (window, $) {
    'use strict';

    // Namespace principal
    window.App = {
        // Configuración global
        config: {
            debug: false,
            apiBaseUrl: '',
            currency: 'ARS',
            dateFormat: 'DD/MM/YYYY'
        },

        // Formateo común
        format: {
            currency: function (value) {
                return new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: App.config.currency
                }).format(value);
            },
            date: function (date) {
                return new Date(date).toLocaleDateString('es-AR');
            }
        },

        // Inicialización de la aplicación
        init: function () {
            if (App.config.debug) {
                console.log('Inicializando App...');
            }

            // Garantizar que debug es una función
            this.debug = function (message, data) {
                if (this.config.debug) {
                    console.log(message, data || '');
                }
            };

            // Inicializar submódulos
            this.initComponents();

            // Configurar manejadores globales
            this.setupGlobalHandlers();
        },

        // Inicializar componentes según disponibilidad
        initComponents: function () {
            if (App.forms) App.forms.init();
            if (App.tables) App.tables.init();
            if (App.products) App.products.init();
            if (App.permissions) App.permissions.init();
        },

        // Manejadores globales
        setupGlobalHandlers: function () {
            // Confirmación de eliminación
            $(document).on('click', 'a[href*="Delete"], .btn-danger[href*="Delete"]', function (e) {
                if (!confirm('¿Está seguro que desea eliminar este elemento? Esta acción no se puede deshacer.')) {
                    e.preventDefault();
                    return false;
                }
                return true;
            });
        },

        // Utilidades AJAX con soporte anti-forgery token
        ajax: {
            getToken: function () {
                return $('input[name="__RequestVerificationToken"]').val();
            },

            get: function (url, data, successCallback, errorCallback) {
                $.ajax({
                    url: url,
                    type: 'GET',
                    data: data,
                    success: successCallback,
                    error: errorCallback || function (xhr, status, error) {
                        console.error("Error en solicitud GET:", url, status, error);
                        App.notify.error('Error al procesar la solicitud.');
                    }
                });
            },

            post: function (url, data, successCallback, errorCallback) {
                const token = this.getToken();

                // Añadir token a los datos si es objeto
                if (data && typeof data === 'object' && !(data instanceof FormData)) {
                    data.__RequestVerificationToken = token;
                }

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: data,
                    headers: {
                        'RequestVerificationToken': token
                    },
                    success: successCallback,
                    error: errorCallback || function (xhr, status, error) {
                        console.error("Error en solicitud POST:", url, status, error);

                        let errorMessage = 'Error al procesar la solicitud.';
                        try {
                            const response = JSON.parse(xhr.responseText);
                            if (response && response.message) {
                                errorMessage = response.message;
                            }
                        } catch (e) {
                            errorMessage = xhr.statusText || errorMessage;
                        }

                        App.notify.error(errorMessage);
                    }
                });
            }
        },

        // Sistema de notificaciones
        notify: {
            success: function (message) {
                if (typeof toastr !== 'undefined') {
                    toastr.success(message);
                } else {
                    alert(message);
                }
            },

            error: function (message) {
                if (typeof toastr !== 'undefined') {
                    toastr.error(message);
                } else {
                    alert('Error: ' + message);
                }
            },

            warning: function (message) {
                if (typeof toastr !== 'undefined') {
                    toastr.warning(message);
                } else {
                    alert('Advertencia: ' + message);
                }
            },

            info: function (message) {
                if (typeof toastr !== 'undefined') {
                    toastr.info(message);
                } else {
                    alert('Info: ' + message);
                }
            }
        },

        // Debug
        debug: function (message, data) {
            if (App.config.debug) {
                console.log(message, data || '');
            }
        }
    };

    // Inicializar al cargar el documento
    $(document).ready(function () {
        App.init();
    });

})(window, jQuery);