// payment-method-handler.js - Módulo para manejo de formas de pago
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.paymentMethodHandler = {
        init: function (options) {
            options = options || {};
            var prefix = options.prefix || '';
            var onChangeCallback = options.onChange;

            // Selector para el dropdown de forma de pago
            var paymentSelect = $('#' + prefix + 'FormaPagoID');

            // Configurar el cambio de forma de pago
            paymentSelect.on('change', function () {
                const formaPagoID = parseInt($(this).val());

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor correspondiente
                switch (formaPagoID) {
                    case 2: // Tarjeta de Crédito
                        $('#' + prefix + 'tarjetaCreditoContainer').removeClass('d-none');
                        break;
                    case 3: // Tarjeta de Débito
                        $('#' + prefix + 'tarjetaDebitoContainer').removeClass('d-none');
                        break;
                    case 4: // Transferencia
                        $('#' + prefix + 'transferenciaContainer').removeClass('d-none');
                        break;
                    case 5: // Pago Virtual
                        $('#' + prefix + 'pagoVirtualContainer').removeClass('d-none');
                        break;
                    case 6: // Crédito Personal
                        $('#' + prefix + 'creditoPersonalContainer').removeClass('d-none');
                        break;
                    case 7: // Cheque
                        $('#' + prefix + 'chequeContainer').removeClass('d-none');
                        break;
                }

                // Ejecutar callback si existe
                if (typeof onChangeCallback === 'function') {
                    onChangeCallback(formaPagoID);
                }
            });

            // Ejecutar cambio inicial
            paymentSelect.trigger('change');
        }
    };

})(window, jQuery);