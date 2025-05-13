// core.js - Core utilities for the application
(function (window, $) {
    'use strict';

    // Application namespace
    window.App = window.App || {};

    // Format utilities
    App.formatCurrency = function (value) {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }).format(value);
    };

    // AJAX utilities with anti-forgery token support
    App.ajax = {
        get: function (url, data, successCallback, errorCallback) {
            $.ajax({
                url: url,
                type: 'GET',
                data: data,
                success: successCallback,
                error: errorCallback || function () {
                    console.error("Error en solicitud GET:", url);
                    alert('Error al procesar la solicitud.');
                }
            });
        },
        post: function (url, data, successCallback, errorCallback) {
            const token = $('input[name="__RequestVerificationToken"]').val();

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
                    alert('Error al procesar la solicitud.');
                }
            });
        }
    };

    // Notification utilities
    App.notify = {
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
        }
    };

    // Initialize app
    App.init = function () {
        $(document).ready(function () {
            // Initialize any global components
            if (typeof App.forms !== 'undefined') App.forms.init();
            if (typeof App.tables !== 'undefined') App.tables.init();

            // Apply confirmation to delete buttons
            $('a[href*="Delete"], .btn-danger[href*="Delete"]').on('click', function (e) {
                if (!confirm('¿Está seguro que desea eliminar este elemento? Esta acción no se puede deshacer.')) {
                    e.preventDefault();
                    return false;
                }
                return true;
            });
        });
    };

})(window, jQuery);

// Initialize the application
document.addEventListener('DOMContentLoaded', function () {
    if (window.App && typeof window.App.init === 'function') {
        window.App.init();
    }
});