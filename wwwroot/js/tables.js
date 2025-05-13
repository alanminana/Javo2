// tables.js - Table operations
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};
    App.tables = {};

    // Initialize tables
    App.tables.init = function () {
        // Initialize event handlers for tables
        initDeleteButton();
        initQuantityChange();
    };

    // Setup delete row button handler
    function initDeleteButton() {
        $(document).on('click', '.eliminar-producto, .remove-product', function () {
            const $row = $(this).closest('tr');
            const $table = $row.closest('table');

            $row.remove();

            // Update totals if applicable
            const tableId = $table.attr('id');
            if (tableId) {
                App.tables.reindexRows(tableId);
                App.tables.updateTotals(tableId);
            }
        });
    }

    // Setup quantity change handler
    function initQuantityChange() {
        $(document).on('change', '.cantidad', function () {
            const $row = $(this).closest('tr');
            const $table = $row.closest('table');
            const cantidad = parseInt($(this).val());
            const precio = parseFloat($row.find('input[name$=".PrecioUnitario"]').val());
            const subtotal = cantidad * precio;

            $row.find('.subtotal').text(App.formatCurrency(subtotal));
            $row.find('input[name$=".PrecioTotal"]').val(subtotal);

            // Update table totals
            const tableId = $table.attr('id');
            if (tableId) {
                App.tables.updateTotals(tableId);
            }
        });
    }

    // Reindex table rows for form submission
    App.tables.reindexRows = function (tableId) {
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
    };

    // Update table totals
    App.tables.updateTotals = function (tableId, options) {
        options = options || {};

        let totalProducts = 0;
        let totalAmount = 0;

        $(`#${tableId} tbody tr`).each(function () {
            const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
            const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
            const subtotal = cantidad * precio;

            totalProducts += cantidad;
            totalAmount += subtotal;

            // Update hidden field for form submission
            $(this).find('input[name$=".PrecioTotal"]').val(subtotal);
        });

        // Update total displays
        const totalProductsId = options.totalProductsId || 'totalProductos';
        const totalAmountId = options.totalAmountId || 'totalVenta';

        $(`#${totalProductsId}`).text(totalProducts);
        $(`#${totalAmountId}`).text(App.formatCurrency(totalAmount));

        // Set hidden total input if provided
        if (options.hiddenTotalInput) {
            $(options.hiddenTotalInput).val(totalAmount);
        }

        return {
            products: totalProducts,
            amount: totalAmount
        };
    };

    // Toggle select all functionality
    App.tables.initSelectAll = function (checkAllSelector, itemSelector) {
        $(document).on('change', checkAllSelector, function () {
            const isChecked = $(this).prop('checked');
            $(itemSelector).prop('checked', isChecked);

            // Highlight rows if needed
            if ($(itemSelector).closest('tr').length) {
                $(itemSelector).closest('tr').toggleClass('table-active', isChecked);
            }
        });

        // Individual checkbox change
        $(document).on('change', itemSelector, function () {
            const allChecked = $(itemSelector).length === $(itemSelector + ':checked').length;
            $(checkAllSelector).prop('checked', allChecked);

            // Highlight row
            $(this).closest('tr').toggleClass('table-active', this.checked);
        });
    };

})(window, jQuery);