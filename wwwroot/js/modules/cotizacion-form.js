// cotizacion-form.js - Módulo para formulario de cotizaciones
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    // Heredamos la mayor parte de la funcionalidad del formulario de ventas
    App.cotizacionForm = Object.create(App.ventaForm);

    // Sobreescribimos o extendemos métodos según necesidad
    App.cotizacionForm.init = function () {
        // Llamar a la inicialización del padre
        App.ventaForm.init.call(this);

        // Configuraciones específicas para cotizaciones
        this.setupCotizacionActions();
    };

    App.cotizacionForm.setupCotizacionActions = function () {
        // Convertir cotización a venta
        $('#convertirAVenta').click(function (e) {
            e.preventDefault();

            if (!confirm('¿Está seguro que desea convertir esta cotización en una venta?')) {
                return;
            }

            const cotizacionId = $(this).data('id');

            // Serializar el formulario actual
            const formData = $('#cotizacionForm').serializeArray();
            $('#ventaData').val(JSON.stringify(formData));

            // Enviar formulario de conversión
            $('#convertirForm').submit();
        });

        // Actualizar cotización
        $('#actualizarCotizacion').click(function (e) {
            e.preventDefault();

            // Validar productos
            if ($('#productosTable tbody tr').length === 0) {
                App.notify.warning('Debe agregar al menos un producto para actualizar la cotización');
                return;
            }

            // Enviar formulario
            $('#cotizacionForm').submit();
        });
    };

})(window, jQuery);