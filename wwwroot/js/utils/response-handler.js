// utils/response-handler.js
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.response = {
        // Manejador de éxito estándar
        success: function (response, options) {
            options = options || {};

            // Si la respuesta tiene formato estándar
            if (response && typeof response === 'object') {
                if (response.success) {
                    // Mostrar mensaje de éxito
                    if (response.message && options.showMessage !== false) {
                        App.notify.success(response.message);
                    }

                    // Ejecutar callback de éxito
                    if (options.onSuccess && typeof options.onSuccess === 'function') {
                        options.onSuccess(response.data || response);
                    }

                    // Redireccionar si es necesario
                    if (response.redirectUrl && options.allowRedirect !== false) {
                        setTimeout(function () {
                            window.location.href = response.redirectUrl;
                        }, options.redirectDelay || 500);
                    }

                    // Recargar si es necesario
                    if (response.reload && options.allowReload !== false) {
                        setTimeout(function () {
                            window.location.reload();
                        }, options.reloadDelay || 500);
                    }

                    return true;
                } else {
                    // Es un objeto pero con error
                    this.error(response, options);
                    return false;
                }
            }

            // Respuesta simple
            if (options.onSuccess && typeof options.onSuccess === 'function') {
                options.onSuccess(response);
            }

            return true;
        },

        // Manejador de error estándar
        error: function (response, options) {
            options = options || {};

            let errorMessage = 'Ha ocurrido un error.';

            // Determinar mensaje de error
            if (response && typeof response === 'object') {
                if (response.message) {
                    errorMessage = response.message;
                } else if (response.error) {
                    errorMessage = response.error;
                } else if (response.statusText) {
                    errorMessage = response.statusText;
                }

                // Errores de validación
                if (response.errors) {
                    this.handleValidationErrors(response.errors, options);
                }
            } else if (typeof response === 'string') {
                errorMessage = response;
            }

            // Mostrar mensaje de error
            if (options.showMessage !== false) {
                App.notify.error(errorMessage);
            }

            // Ejecutar callback de error
            if (options.onError && typeof options.onError === 'function') {
                options.onError(response);
            }

            return false;
        },

        // Manejar errores de validación
        handleValidationErrors: function (errors, options) {
            options = options || {};
            const formSelector = options.formSelector || 'form';

            // Limpiar errores anteriores
            $(formSelector + ' .is-invalid').removeClass('is-invalid');
            $(formSelector + ' .invalid-feedback').hide();

            // Agregar nuevos errores
            $.each(errors, function (field, messages) {
                const $field = $(formSelector + ' [name="' + field + '"]');
                const message = Array.isArray(messages) ? messages[0] : messages;

                $field.addClass('is-invalid');

                // Buscar o crear contenedor de mensaje
                let $errorContainer = $field.next('.invalid-feedback');
                if ($errorContainer.length === 0) {
                    $errorContainer = $('<div class="invalid-feedback"></div>');
                    $field.after($errorContainer);
                }

                $errorContainer.text(message).show();
            });
        }
    };

})(window, jQuery);