// tables.js - Módulo para operaciones con tablas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.tables = {
        init: function () {
            this.initDeleteButton();
            this.initQuantityChange();
        },

        // Configurar botón de eliminar fila
        initDeleteButton: function () {
            $(document).on('click', '.eliminar-producto, .remove-product', function () {
                const $row = $(this).closest('tr');
                const $table = $row.closest('table');

                $row.remove();

                // Actualizar totales si aplica
                const tableId = $table.attr('id');
                if (tableId) {
                    App.tables.reindexRows(tableId);
                    App.tables.updateTotals(tableId);
                }
            });
        },

        // Configurar cambio de cantidad
        initQuantityChange: function () {
            $(document).on('change', '.cantidad', function () {
                const $row = $(this).closest('tr');
                const $table = $row.closest('table');
                const cantidad = parseInt($(this).val());
                const precio = parseFloat($row.find('input[name$=".PrecioUnitario"]').val());
                const subtotal = cantidad * precio;

                $row.find('.subtotal').text(App.format.currency(subtotal));
                $row.find('input[name$=".PrecioTotal"]').val(subtotal);

                // Actualizar totales de tabla
                const tableId = $table.attr('id');
                if (tableId) {
                    App.tables.updateTotals(tableId);
                }
            });
        },

        // Reindexar filas para envío del formulario
        reindexRows: function (tableId) {
            $(`#${tableId} tbody tr`).each(function (index) {
                $(this).attr('data-index', index);

                $(this).find('input').each(function () {
                    const name = $(this).attr('name');
                    if (name && name.includes('[')) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            });
        },

        // Actualizar totales
        updateTotals: function (tableId, options) {
            options = options || {};

            let totalProducts = 0;
            let totalAmount = 0;

            $(`#${tableId} tbody tr`).each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                totalProducts += cantidad;
                totalAmount += subtotal;

                // Actualizar campo oculto para envío del formulario
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal);
            });

            // Actualizar totales en la UI
            const totalProductsId = options.totalProductsId || 'totalProductos';
            const totalAmountId = options.totalAmountId || 'totalVenta';

            $(`#${totalProductsId}`).text(totalProducts);
            $(`#${totalAmountId}`).text(App.format.currency(totalAmount));

            // Establecer total oculto si se proporciona
            if (options.hiddenTotalInput) {
                $(options.hiddenTotalInput).val(totalAmount);
            }

            return {
                products: totalProducts,
                amount: totalAmount
            };
        },

        // Toggle seleccionar todo
        initSelectAll: function (checkAllSelector, itemSelector) {
            $(document).on('change', checkAllSelector, function () {
                const isChecked = $(this).prop('checked');
                $(itemSelector).prop('checked', isChecked);

                // Resaltar filas si es necesario
                if ($(itemSelector).closest('tr').length) {
                    $(itemSelector).closest('tr').toggleClass('table-active', isChecked);
                }
            });

            // Cambio de checkbox individual
            $(document).on('change', itemSelector, function () {
                const allChecked = $(itemSelector).length === $(itemSelector + ':checked').length;
                $(checkAllSelector).prop('checked', allChecked);

                // Resaltar fila
                $(this).closest('tr').toggleClass('table-active', this.checked);
            });
        }
    };

})(window, jQuery);