// products.js - Product operations and search
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};
    App.products = {};

    // Product search via code and UI updates
    App.products.searchByCode = function (url, codeValue, options) {
        options = options || {};

        if (!codeValue) {
            if (options.onError) options.onError('Ingrese un código para buscar');
            return;
        }

        App.ajax.post(url, { codigoProducto: codeValue }, function (response) {
            if (response.success) {
                const product = {
                    id: response.data.productoID,
                    codigoAlfa: response.data.codigoAlfa,
                    codigoBarra: response.data.codigoBarra,
                    nombre: response.data.nombreProducto,
                    marca: response.data.marca,
                    precio: response.data.precioUnitario,
                    precioLista: response.data.precioLista
                };

                // Update UI fields if provided
                if (options.nameField) $(options.nameField).val(product.nombre);
                if (options.priceField) $(options.priceField).val(product.precio);
                if (options.quantityField) {
                    $(options.quantityField).val(1);
                    $(options.quantityField).focus();
                }

                if (options.onSuccess) options.onSuccess(product);
            } else {
                // Error handling
                if (options.modalErrorId) {
                    $(options.modalErrorId).modal('show');
                } else if (options.onError) {
                    options.onError('Producto no encontrado');
                } else {
                    alert('Producto no encontrado');
                }

                // Clear fields
                if (options.nameField) $(options.nameField).val('');
                if (options.priceField) $(options.priceField).val('');

                if (options.onNotFound) options.onNotFound();
            }
        }, function () {
            // Ajax error
            if (options.onError) options.onError('Error al buscar producto');
            else alert('Error al buscar producto');
        });
    };

    // Add product to table with validation
    App.products.addToTable = function (tableId, product, quantity, options) {
        options = options || {};

        if (!product || !product.id) {
            alert('Debe buscar un producto primero');
            return false;
        }

        const qty = parseInt(quantity);
        if (isNaN(qty) || qty <= 0) {
            alert('La cantidad debe ser mayor a cero');
            return false;
        }

        // Check if product already exists in table
        let exists = false;
        let rowIndex = -1;

        $(`#${tableId} tbody tr`).each(function (i) {
            const pid = $(this).find('input[name$=".ProductoID"]').val();
            if (parseInt(pid) === product.id) {
                exists = true;
                rowIndex = i;
                return false; // Break the loop
            }
        });

        const subtotal = qty * product.precio;

        if (exists) {
            // Update existing row
            const $row = $(`#${tableId} tbody tr`).eq(rowIndex);
            const currentQty = parseInt($row.find('.cantidad').val());
            const newQty = currentQty + qty;

            $row.find('.cantidad').val(newQty);
            $row.find('.subtotal').text(App.formatCurrency(newQty * product.precio));
            $row.find('input[name$=".PrecioTotal"]').val(newQty * product.precio);
        } else {
            // Create new row
            const rowCount = $(`#${tableId} tbody tr`).length;

            const newRow = `
                <tr data-index="${rowCount}">
                    <td>
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].ProductoID" value="${product.id}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoAlfa" value="${product.codigoAlfa || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoBarra" value="${product.codigoBarra || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].Marca" value="${product.marca || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].NombreProducto" value="${product.nombre}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioUnitario" value="${product.precio}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioTotal" value="${subtotal}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioLista" value="${product.precioLista || 0}" />
                        ${product.codigoAlfa || product.codigoBarra || product.id}
                    </td>
                    <td>${product.nombre}</td>
                    <td><input type="number" name="ProductosPresupuesto[${rowCount}].Cantidad" value="${qty}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                    <td>${App.formatCurrency(product.precio)}</td>
                    <td><span class="subtotal">${App.formatCurrency(subtotal)}</span></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `;

            $(`#${tableId} tbody`).append(newRow);
        }

        // Update totals if callback provided
        if (options.updateTotals && typeof options.updateTotals === 'function') {
            options.updateTotals();
        }

        // Reset form fields if requested
        if (options.resetFields) {
            if (options.codeField) $(options.codeField).val('');
            if (options.nameField) $(options.nameField).val('');
            if (options.priceField) $(options.priceField).val('');
        }

        return true;
    };

})(window, jQuery);