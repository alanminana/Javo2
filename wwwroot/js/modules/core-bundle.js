// core-bundle.js - Funcionalidades core de la aplicación consolidadas
(function (window, $) {
    'use strict';

    // Namespace principal
    var App = window.App = window.App || {};

    // CONFIGURACIÓN GLOBAL
    App.config = {
        debug: false,
        apiBaseUrl: '',
        currency: 'ARS',
        dateFormat: 'DD/MM/YYYY',
        version: '1.0.0' // Añadir versión para tracking
    };

    // SISTEMA DE LOGGING Y DEBUGGING MEJORADO
    App.debug = (function () {
        // Función principal de debug
        const logger = function (message, data) {
            if (App.config.debug) {
                console.log(message, data || '');
            }
        };

        // Métodos específicos por nivel
        logger.log = function (message, data) {
            if (App.config.debug) {
                console.log(message, data || '');
            }
        };

        logger.info = function (message, data) {
            if (App.config.debug) {
                console.info(message, data || '');
            }
        };

        logger.warn = function (message, data) {
            if (App.config.debug) {
                console.warn(message, data || '');
            } else if (App.config.warnInProduction) {
                // Opción para mostrar warnings incluso en producción
                console.warn(message, data || '');
            }
        };

        logger.error = function (message, data) {
            // Los errores siempre se loggean, independientemente del modo debug
            console.error(message, data || '');

            // Opcionalmente enviar a servicio de telemetría
            if (App.config.errorTracking && typeof App.telemetry !== 'undefined') {
                App.telemetry.trackError(message, data);
            }
        };

        logger.group = function (title) {
            if (App.config.debug && console.group) {
                console.group(title);
            }
        };

        logger.groupEnd = function () {
            if (App.config.debug && console.groupEnd) {
                console.groupEnd();
            }
        };

        logger.time = function (label) {
            if (App.config.debug && console.time) {
                console.time(label);
            }
        };

        logger.timeEnd = function (label) {
            if (App.config.debug && console.timeEnd) {
                console.timeEnd(label);
            }
        };

        return logger;
    })();

    // UTILIDADES DE FORMATEO
    App.format = {
        // Formateo de moneda
        currency: function (value) {
            if (isNaN(value)) return '0,00';

            return new Intl.NumberFormat('es-AR', {
                style: 'currency',
                currency: App.config.currency,
                minimumFractionDigits: 2
            }).format(value);
        },

        // Formateo de fecha
        date: function (date, format) {
            if (!date) return '';

            const d = new Date(date);
            if (isNaN(d.getTime())) return '';

            // Si está disponible moment.js, usarlo
            if (typeof moment !== 'undefined') {
                return moment(d).format(format || App.config.dateFormat);
            }

            // Fallback a implementación básica
            const pad = (num) => num.toString().padStart(2, '0');

            const day = pad(d.getDate());
            const month = pad(d.getMonth() + 1);
            const year = d.getFullYear();

            switch (format) {
                case 'MM/DD/YYYY':
                    return `${month}/${day}/${year}`;
                case 'YYYY-MM-DD':
                    return `${year}-${month}-${day}`;
                default:
                    return `${day}/${month}/${year}`;
            }
        },

        // Formateo de números
        number: function (value, decimals = 2) {
            if (isNaN(value)) return '0';

            return new Intl.NumberFormat('es-AR', {
                minimumFractionDigits: decimals,
                maximumFractionDigits: decimals
            }).format(value);
        },

        // Formateo para mostrar KB, MB, GB
        fileSize: function (bytes) {
            if (bytes === 0) return '0 Bytes';

            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));

            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        },

        // Truncar texto con ellipsis
        truncate: function (text, length = 100) {
            if (!text) return '';
            if (text.length <= length) return text;

            return text.substring(0, length) + '...';
        }
    };

    // UTILIDADES AJAX CON MEJORAS
    App.ajax = {
        // Obtener token anti-forgery
        getToken: function () {
            return $('input[name="__RequestVerificationToken"]').val();
        },

        // GET request
        get: function (url, data, successCallback, errorCallback) {
            App.debug.time('AJAX GET: ' + url);

            $.ajax({
                url: url,
                type: 'GET',
                data: data,
                success: function (response) {
                    App.debug.timeEnd('AJAX GET: ' + url);
                    if (successCallback && typeof successCallback === 'function') {
                        successCallback(response);
                    }
                },
                error: function (xhr, status, error) {
                    App.debug.timeEnd('AJAX GET: ' + url);
                    App.debug.error("Error en solicitud GET:", { url, status, error, responseText: xhr.responseText });

                    if (errorCallback && typeof errorCallback === 'function') {
                        errorCallback(xhr, status, error);
                    } else {
                        App.notify.error('Error al procesar la solicitud.');
                    }
                }
            });
        },

        // POST request
        post: function (url, data, successCallback, errorCallback) {
            const token = this.getToken();
            App.debug.time('AJAX POST: ' + url);

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
                success: function (response) {
                    App.debug.timeEnd('AJAX POST: ' + url);
                    if (successCallback && typeof successCallback === 'function') {
                        successCallback(response);
                    }
                },
                error: function (xhr, status, error) {
                    App.debug.timeEnd('AJAX POST: ' + url);
                    App.debug.error("Error en solicitud POST:", { url, status, error, responseText: xhr.responseText });

                    let errorMessage = 'Error al procesar la solicitud.';
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response && response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        errorMessage = xhr.statusText || errorMessage;
                    }

                    if (errorCallback && typeof errorCallback === 'function') {
                        errorCallback(xhr, status, error);
                    } else {
                        App.notify.error(errorMessage);
                    }
                }
            });
        },

        // JSON POST (automaticamente convierte data a JSON)
        postJSON: function (url, data, successCallback, errorCallback) {
            const token = this.getToken();
            App.debug.time('AJAX JSON POST: ' + url);

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                headers: {
                    'RequestVerificationToken': token
                },
                success: function (response) {
                    App.debug.timeEnd('AJAX JSON POST: ' + url);
                    if (successCallback && typeof successCallback === 'function') {
                        successCallback(response);
                    }
                },
                error: function (xhr, status, error) {
                    App.debug.timeEnd('AJAX JSON POST: ' + url);
                    App.debug.error("Error en solicitud JSON POST:", { url, status, error, responseText: xhr.responseText });

                    let errorMessage = 'Error al procesar la solicitud.';
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response && response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        errorMessage = xhr.statusText || errorMessage;
                    }

                    if (errorCallback && typeof errorCallback === 'function') {
                        errorCallback(xhr, status, error);
                    } else {
                        App.notify.error(errorMessage);
                    }
                }
            });
        }
    };

    // SISTEMA DE NOTIFICACIONES MEJORADO
    App.notify = {
        // Config
        config: {
            duration: 5000,
            position: 'toast-top-right',
            showProgress: true,
            closeButton: true
        },

        // Success notification
        success: function (message, title, options) {
            this._notify('success', message, title, options);
        },

        // Error notification
        error: function (message, title, options) {
            this._notify('error', message, title, options);
        },

        // Warning notification
        warning: function (message, title, options) {
            this._notify('warning', message, title, options);
        },

        // Info notification
        info: function (message, title, options) {
            this._notify('info', message, title, options);
        },

        // Método centralizado para mostrar notificaciones
        _notify: function (type, message, title, options) {
            options = options || {};

            // Usar toastr si está disponible
            if (typeof toastr !== 'undefined') {
                // Configurar toastr
                toastr.options = $.extend({}, this.config, options);

                // Mostrar notificación
                toastr[type](message, title);
            } else {
                // Fallback a alerts nativos
                let fullMessage = '';
                if (title) {
                    fullMessage = title + ': ';
                }
                fullMessage += message;

                switch (type) {
                    case 'error':
                        alert('Error: ' + fullMessage);
                        break;
                    case 'warning':
                        alert('Advertencia: ' + fullMessage);
                        break;
                    case 'info':
                    case 'success':
                    default:
                        alert(fullMessage);
                        break;
                }
            }
        },

        // Mostrar notificación desde respuesta HTTP
        fromResponse: function (response) {
            if (!response) return;

            if (response.success === true && response.message) {
                this.success(response.message);
            } else if (response.success === false && response.message) {
                this.error(response.message);
            } else if (typeof response === 'string') {
                this.info(response);
            }
        }
    };

    // UTILIDADES DOM Y MANIPULACIÓN
    App.dom = {
        // Scrollear a un elemento con animación
        scrollTo: function (element, offset = 0, speed = 500) {
            if (!element) return;

            const $element = $(element);
            if (!$element.length) return;

            $('html, body').animate({
                scrollTop: $element.offset().top - offset
            }, speed);
        },

        // Marcar campo como inválido
        markInvalid: function (field, message) {
            const $field = $(field);
            if (!$field.length) return;

            $field.addClass('is-invalid');

            // Añadir mensaje de error si no existe
            let $feedback = $field.next('.invalid-feedback');
            if (!$feedback.length) {
                $feedback = $('<div class="invalid-feedback"></div>');
                $field.after($feedback);
            }

            $feedback.text(message || 'Este campo es requerido');
        },

        // Limpiar validación
        clearValidation: function (container) {
            const $container = container ? $(container) : $('form');

            $container.find('.is-invalid').removeClass('is-invalid');
            $container.find('.is-valid').removeClass('is-valid');
            $container.find('.invalid-feedback, .valid-feedback').hide();
        },

        // Serializar formulario a objeto
        serializeObject: function (form) {
            const $form = $(form);
            if (!$form.length) return {};

            const formData = $form.serializeArray();
            const result = {};

            $.each(formData, function () {
                if (result[this.name]) {
                    if (!result[this.name].push) {
                        result[this.name] = [result[this.name]];
                    }
                    result[this.name].push(this.value || '');
                } else {
                    result[this.name] = this.value || '';
                }
            });

            return result;
        },

        // Mostrar/ocultar loader
        showLoader: function (container, message) {
            const $container = $(container);
            if (!$container.length) return;

            const html = `
                <div class="app-loader text-center p-3">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-2">${message || 'Cargando...'}</p>
                </div>
            `;

            // Guardar contenido original
            $container.data('original-content', $container.html());
            $container.html(html);
        },

        hideLoader: function (container) {
            const $container = $(container);
            if (!$container.length) return;

            // Restaurar contenido original
            const originalContent = $container.data('original-content');
            if (originalContent) {
                $container.html(originalContent);
                $container.removeData('original-content');
            } else {
                $container.find('.app-loader').remove();
            }
        }
    };

    // INICIALIZACIÓN
    App.init = function () {
        App.debug.log('Inicializando App Core...');

        // Configurar manejadores globales
        this.setupGlobalHandlers();

        // Emitir evento que indica que el core está listo
        $(document).trigger('app:ready');
    };

    // MANEJADORES GLOBALES
    App.setupGlobalHandlers = function () {
        // Confirmación de eliminación
        $(document).on('click', 'a[href*="Delete"], .btn-danger[href*="Delete"]', function (e) {
            if (!confirm('¿Está seguro que desea eliminar este elemento? Esta acción no se puede deshacer.')) {
                e.preventDefault();
                return false;
            }
            return true;
        });

        // Manejar errores AJAX globales
        $(document).ajaxError(function (event, jqXHR, settings, error) {
            // Evitar duplicar errores que ya son manejados específicamente
            if (settings.hasOwnProperty('_errorHandled') && settings._errorHandled) {
                return;
            }

            // Manejar errores comunes
            if (jqXHR.status === 401) {
                App.notify.error('Su sesión ha expirado. Por favor vuelva a iniciar sesión.');
                setTimeout(() => { window.location.href = '/Account/Login'; }, 2000);
            } else if (jqXHR.status === 403) {
                App.notify.error('No tiene permisos para realizar esta operación.');
            } else if (jqXHR.status === 404) {
                App.notify.error('El recurso solicitado no fue encontrado.');
            } else if (jqXHR.status === 500) {
                App.notify.error('Ha ocurrido un error en el servidor. Por favor contacte al administrador.');
            }
        });

        // Inicializar tooltips y popovers de Bootstrap si están disponibles
        if (typeof bootstrap !== 'undefined') {
            // Inicializar tooltips
            const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            if (tooltips.length) {
                Array.from(tooltips).forEach(tooltip => {
                    new bootstrap.Tooltip(tooltip);
                });
            }

            // Inicializar popovers
            const popovers = document.querySelectorAll('[data-bs-toggle="popover"]');
            if (popovers.length) {
                Array.from(popovers).forEach(popover => {
                    new bootstrap.Popover(popover);
                });
            }
        }
    };

    // Inicializar cuando el DOM esté listo
    $(document).ready(function () {
        App.init();
    });

})(window, jQuery);