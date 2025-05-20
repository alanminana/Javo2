// notification.js - Sistema unificado de notificaciones
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

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

    // Inicializar cuando el documento esté listo
    $(document).ready(function () {
        App.notify.init();
    });

})(window, jQuery);