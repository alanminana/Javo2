// common.js - Utilidades básicas consolidadas
(function (window, $) {
    'use strict';

    // Namespace principal
    var App = window.App = window.App || {};

    // Configuración global de la aplicación
    App.config = {
        debug: false,
        apiBaseUrl: '',
        currency: 'ARS',
        dateFormat: 'DD/MM/YYYY',
        version: '2.0.0'
    };

    // =========================================================================
    // FORMATEO
    // =========================================================================
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

    // =========================================================================
    // VALIDACIÓN
    // =========================================================================
    App.validation = {
        // Validar campos requeridos en un formulario
        validateRequired: function (formSelector, options) {
            options = options || {};
            let isValid = true;

            const errorClass = options.errorClass || 'is-invalid';
            const validClass = options.validClass || 'is-valid';
            const errorMessage = options.errorMessage || 'Este campo es obligatorio';

            $(formSelector + ' [required]').each(function () {
                const $field = $(this);
                const value = $field.val();

                if (!value || value.trim() === '') {
                    isValid = false;
                    $field.addClass(errorClass).removeClass(validClass);

                    // Verificar si ya existe el mensaje de error
                    let $errorMsg = $field.next('.invalid-feedback');
                    if ($errorMsg.length === 0) {
                        $errorMsg = $('<div class="invalid-feedback"></div>');
                        $field.after($errorMsg);
                    }

                    $errorMsg.text(errorMessage).show();
                } else {
                    $field.removeClass(errorClass);
                    if (options.showValid) {
                        $field.addClass(validClass);
                    }
                    $field.next('.invalid-feedback').hide();
                }
            });

            return isValid;
        },

        // Validar correo electrónico
        isValidEmail: function (email) {
            const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(String(email).toLowerCase());
        },

        // Validar DNI argentino
        isValidDNI: function (dni) {
            const cleaned = dni.toString().replace(/\D/g, '');
            return cleaned.length >= 7 && cleaned.length <= 8 && !isNaN(cleaned);
        },

        // Validar número telefónico
        isValidPhone: function (phone) {
            const cleaned = phone.toString().replace(/\D/g, '');
            return cleaned.length >= 8;
        }
    };

    // =========================================================================
    // AJAX
    // =========================================================================
    App.ajax = {
        // Obtener token anti-forgery
        getToken: function () {
            return $('input[name="__RequestVerificationToken"]').val();
        },

        // Realizar petición GET
        get: function (url, data, successCallback, errorCallback) {
            const options = {
                url: url,
                type: 'GET',
                data: data,
                success: successCallback,
                error: errorCallback || this.defaultErrorHandler
            };

            $.ajax(options);
        },

        // Realizar petición POST
        post: function (url, data, successCallback, errorCallback) {
            const token = this.getToken();
            const isFormData = data instanceof FormData;

            // Añadir token a los datos si no es FormData
            if (data && typeof data === 'object' && !isFormData) {
                data.__RequestVerificationToken = token;
            }

            const options = {
                url: url,
                type: 'POST',
                data: data,
                headers: {
                    'RequestVerificationToken': token
                },
                success: successCallback,
                error: errorCallback || this.defaultErrorHandler
            };

            // Si es FormData, configurar opciones adicionales
            if (isFormData) {
                options.processData = false;
                options.contentType = false;
            }

            $.ajax(options);
        },

        // Manejador de errores por defecto
        defaultErrorHandler: function (xhr, status, error) {
            console.error("Error en solicitud AJAX:", status, error);

            let errorMessage = 'Error al procesar la solicitud.';
            try {
                const response = JSON.parse(xhr.responseText);
                if (response && response.message) {
                    errorMessage = response.message;
                }
            } catch (e) {
                errorMessage = xhr.statusText || errorMessage;
            }

            if (App.notify) {
                App.notify.error(errorMessage);
            } else {
                alert('Error: ' + errorMessage);
            }
        }
    };

    // =========================================================================
    // NOTIFICACIONES
    // =========================================================================
    App.notify = {
        // Configuración por defecto
        config: {
            position: 'top-right',
            timeOut: 5000,
            closeButton: true,
            progressBar: true,
            preventDuplicates: true
        },

        // Inicializar toastr si está disponible
        init: function (options) {
            if (typeof toastr !== 'undefined') {
                toastr.options = $.extend({}, this.config, options);
            }
        },

        // Mensaje de éxito
        success: function (message, title, options) {
            this._showNotification('success', message, title, options);
            return this;
        },

        // Mensaje de error
        error: function (message, title, options) {
            this._showNotification('error', message, title, options);
            return this;
        },

        // Mensaje de advertencia
        warning: function (message, title, options) {
            this._showNotification('warning', message, title, options);
            return this;
        },

        // Mensaje informativo
        info: function (message, title, options) {
            this._showNotification('info', message, title, options);
            return this;
        },

        // Mostrar notificación
        _showNotification: function (type, message, title, options) {
            options = options || {};

            // Si toastr está disponible, usar toastr
            if (typeof toastr !== 'undefined') {
                toastr[type](message, title, options);
                return;
            }

            // Si bootstrap modal está disponible, intentar usar modal
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                this._showBootstrapNotification(type, message, title);
                return;
            }

            // Fallback: usar alert
            const alertPrefix = {
                'success': '✓ ',
                'error': '✗ ',
                'warning': '⚠ ',
                'info': 'ℹ '
            };

            const alertMessage = (title ? title + ': ' : '') + (alertPrefix[type] || '') + message;
            alert(alertMessage);
        },

        // Mostrar notificación con bootstrap
        _showBootstrapNotification: function (type, message, title) {
            // Mappings de tipo a clase
            const typeClass = {
                'success': 'alert-success',
                'error': 'alert-danger',
                'warning': 'alert-warning',
                'info': 'alert-info'
            };

            // Crear elemento de notificación
            const notification = document.createElement('div');
            notification.className = `alert ${typeClass[type] || 'alert-info'} alert-dismissible fade show`;
            notification.setAttribute('role', 'alert');

            // Añadir contenido
            if (title) {
                const strong = document.createElement('strong');
                strong.textContent = title;
                notification.appendChild(strong);
                notification.appendChild(document.createTextNode(': '));
            }

            notification.appendChild(document.createTextNode(message));

            // Botón de cerrar
            const closeButton = document.createElement('button');
            closeButton.type = 'button';
            closeButton.className = 'btn-close';
            closeButton.setAttribute('data-bs-dismiss', 'alert');
            closeButton.setAttribute('aria-label', 'Close');
            notification.appendChild(closeButton);

            // Añadir a la página
            const container = document.querySelector('.toast-container');
            if (container) {
                container.appendChild(notification);
            } else {
                // Crear contenedor si no existe
                const newContainer = document.createElement('div');
                newContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
                newContainer.style.zIndex = '9999';
                newContainer.appendChild(notification);
                document.body.appendChild(newContainer);
            }

            // Auto-eliminar después de 5 segundos
            setTimeout(function () {
                notification.classList.remove('show');
                setTimeout(function () {
                    notification.remove();
                }, 150);
            }, 5000);
        }
    };

    // =========================================================================
    // PERMISOS
    // =========================================================================
    App.permissions = {
        // Caché de permisos
        cache: {},

        // Permisos cargados del servidor
        userPermissions: [],

        // Inicializar sistema de permisos
        init: function () {
            this.loadPermissions();
            this.applyPermissions();
        },

        // Cargar permisos del DOM
        loadPermissions: function () {
            try {
                // Primero intentamos obtener permisos del atributo data-permissions de body
                const permissionsData = document.body.getAttribute('data-permissions');
                if (permissionsData) {
                    this.userPermissions = JSON.parse(permissionsData);
                    return;
                }

                // Si no hay permisos en el body, buscar en elemento específico
                const permData = document.getElementById('userPermissions');
                if (permData && permData.dataset.permissions) {
                    this.userPermissions = JSON.parse(permData.dataset.permissions);
                }
            } catch (e) {
                console.error('Error al parsear permisos:', e);
                this.userPermissions = [];
            }
        },

        // Aplicar permisos a todos los elementos
        applyPermissions: function () {
            // Ocultar elementos sin permiso
            document.querySelectorAll("[data-require-permission]").forEach(element => {
                const permission = element.getAttribute("data-require-permission");
                if (!this.hasPermission(permission)) {
                    element.style.display = "none";
                }
            });

            // Deshabilitar botones sin permiso
            document.querySelectorAll("button[data-require-permission], a[data-require-permission]").forEach(element => {
                const permission = element.getAttribute("data-require-permission");
                if (!this.hasPermission(permission)) {
                    element.disabled = true;
                    element.classList.add("disabled");

                    // Prevenir click en enlaces
                    if (element.tagName === 'A') {
                        element.addEventListener('click', (e) => {
                            e.preventDefault();
                            return false;
                        });
                    }
                }
            });
        },

        // Verificar si el usuario tiene un permiso
        hasPermission: function (permissionCode) {
            if (!permissionCode) return true;

            // Verificar caché primero
            if (this.cache[permissionCode] !== undefined) {
                return this.cache[permissionCode];
            }

            // Verificar permisos cargados
            const result = this.userPermissions.includes(permissionCode);
            this.cache[permissionCode] = result;

            return result;
        }
    };

    // Inicializar cuando el documento esté listo
    $(document).ready(function () {
        if (App.notify) App.notify.init();
        if (App.permissions) App.permissions.init();
    });

})(window, jQuery);