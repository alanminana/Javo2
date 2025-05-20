// validation.js - Utilidades de validación
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

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

        // Validar contraseña
        validatePassword: function (password) {
            return {
                longitud: password.length >= 6,
                letraNumero: /[a-zA-Z]/.test(password) && /[0-9]/.test(password),
                caracterEspecial: /[^a-zA-Z0-9]/.test(password)
            };
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
        },

        // Validar número decimal
        isValidDecimal: function (value) {
            return /^\d+(\.\d+)?$/.test(value);
        },

        // Validar número entero
        isValidInteger: function (value) {
            return /^\d+$/.test(value);
        },

        // Configurar validación de contraseña
        setupPasswordStrength: function (passwordInput, feedbackContainer) {
            $(passwordInput).on('input', function () {
                const valor = $(this).val();
                const validaciones = App.validation.validatePassword(valor);

                let html = '<div class="fw-bold mb-1">Fortaleza de la contraseña:</div>';
                html += '<ul class="mb-0 ps-3">';
                html += `<li class="${validaciones.longitud ? 'text-success' : 'text-danger'}">Al menos 6 caracteres</li>`;
                html += `<li class="${validaciones.letraNumero ? 'text-success' : 'text-danger'}">Letras y números</li>`;
                html += `<li class="${validaciones.caracterEspecial ? 'text-success' : 'text-danger'}">Al menos un carácter especial</li>`;
                html += '</ul>';

                $(feedbackContainer).html(html);
            });
        }
    };

})(window, jQuery);