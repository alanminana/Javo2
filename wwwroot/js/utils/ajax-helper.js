// ajax-helper.js - Utilidades para peticiones AJAX
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

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

        // Realizar petición JSON
        postJSON: function (url, data, successCallback, errorCallback) {
            const token = this.getToken();

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                headers: {
                    'RequestVerificationToken': token
                },
                success: successCallback,
                error: errorCallback || this.defaultErrorHandler
            });
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
        },

        // Serializar formulario a JSON
        serializeToJson: function (formSelector) {
            const formData = $(formSelector).serializeArray();
            const jsonData = {};

            $.each(formData, function () {
                if (jsonData[this.name]) {
                    if (!jsonData[this.name].push) {
                        jsonData[this.name] = [jsonData[this.name]];
                    }
                    jsonData[this.name].push(this.value || '');
                } else {
                    jsonData[this.name] = this.value || '';
                }
            });

            return jsonData;
        }
    };

})(window, jQuery);