// forms.js - Form operations and validation
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};
    App.forms = {};

    // Initialize all forms
    App.forms.init = function () {
        initPaymentForms();
    };

    // Initialize payment form toggles
    function initPaymentForms() {
        $(document).on('change', '.payment-toggle', function () {
            const formaPagoID = parseInt($(this).val());

            // Hide all payment containers
            $('.payment-container').addClass('d-none');

            // Show the selected payment container
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

        // Trigger initial change on page load
        $('.payment-toggle').trigger('change');
    }

    // Client validation helper
    App.forms.validateRequired = function (formSelector) {
        let isValid = true;

        $(formSelector + ' [required]').each(function () {
            const $field = $(this);
            const value = $field.val();

            if (!value || value.trim() === '') {
                isValid = false;
                $field.addClass('is-invalid');

                // Check if error message exists, if not create one
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
    };

    // Form serialization to JSON
    App.forms.serializeToJson = function (formSelector) {
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
    };

})(window, jQuery);