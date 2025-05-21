// forms.js - Módulo de manejo de formularios
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.forms = {
        init: function () {
            this.initPaymentForms();
        },

        // Inicializar formularios de pago
        initPaymentForms: function () {
            $(document).on('change', '.payment-toggle', function () {
                const formaPagoID = parseInt($(this).val());

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor seleccionado
                switch (formaPagoID) {
                    case 2: // Tarjeta de Crédito
                        $('#tarjetaCreditoContainer').removeClass('d-none');
                        break;
                    case 3: // Tarjeta de Débito
                        $('#tarjetaDebitoContainer').removeClass('d-none');
                        break;
                    case 4: // Transferencia
                        $('#transferenciaContainer').removeClass('d-none');
                        break;
                    case 5: // Pago Virtual
                        $('#pagoVirtualContainer').removeClass('d-none');
                        break;
                    case 6: // Crédito Personal
                        $('#creditoPersonalContainer').removeClass('d-none');
                        break;
                    case 7: // Cheque
                        $('#chequeContainer').removeClass('d-none');
                        break;
                }
            });

            // Trigger inicial
            $('.payment-toggle').trigger('change');
        },

        // Validar campos requeridos
        validateRequired: function (formSelector) {
            let isValid = true;

            $(formSelector + ' [required]').each(function () {
                const $field = $(this);
                const value = $field.val();

                if (!value || value.trim() === '') {
                    isValid = false;
                    $field.addClass('is-invalid');

                    // Verificar si ya existe el mensaje de error
                    let $errorMsg = $field.next('.invalid-feedback');
                    if ($errorMsg.length === 0) {
                        $errorMsg = $('<div class="invalid-feedback">Este campo es obligatorio</div>');
                        $field.after($errorMsg);
                    }

                    $errorMsg.show();
                } else {
                    $field.removeClass('is-invalid');
                    $field.next('.invalid-feedback').hide();
                }
            });

            return isValid;
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
        },

        // Validación de contraseñas
        setupPasswordStrength: function (passwordInput, feedbackContainer) {
            $(passwordInput).on('input', function () {
                const valor = $(this).val();
                const validaciones = {
                    longitud: valor.length >= 6,
                    letraNumero: /[a-zA-Z]/.test(valor) && /[0-9]/.test(valor),
                    caracterEspecial: /[^a-zA-Z0-9]/.test(valor)
                };

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