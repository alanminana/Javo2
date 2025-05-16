// products.js - Módulo para operaciones con productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    init: function () {
        // Verifica si exists debug antes de llamarla
        if (App.debug && typeof App.debug === 'function') {
            App.debug('Módulo de productos inicializado');
        } else {
            console.log('Módulo de productos inicializado');
        }
    },

        // Búsqueda de producto por código
        searchByCode: function (url, codeValue, options) {
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

                    // Actualizar campos de UI si se proporcionan
                    if (options.nameField) $(options.nameField).val(product.nombre);
                    if (options.priceField) $(options.priceField).val(product.precio);
                    if (options.quantityField) {
                        $(options.quantityField).val(1);
                        $(options.quantityField).focus();
                    }

                    if (options.onSuccess) options.onSuccess(product);
                } else {
                    // Manejo de error
                    if (options.modalErrorId) {
                        $(options.modalErrorId).modal('show');
                    } else if (options.onError) {
                        options.onError('Producto no encontrado');
                    } else {
                        App.notify.error('Producto no encontrado');
                    }

                    // Limpiar campos
                    if (options.nameField) $(options.nameField).val('');
                    if (options.priceField) $(options.priceField).val('');

                    if (options.onNotFound) options.onNotFound();
                }
            }, function () {
                // Error AJAX
                if (options.onError) options.onError('Error al buscar producto');
                else App.notify.error('Error al buscar producto');
            });
        },

        // Añadir producto a tabla con validación
        addToTable: function (tableId, product, quantity, options) {
            options = options || {};

            if (!product || !product.id) {
                App.notify.warning('Debe buscar un producto primero');
                return false;
            }

            const qty = parseInt(quantity);
            if (isNaN(qty) || qty <= 0) {
                App.notify.warning('La cantidad debe ser mayor a cero');
                return false;
            }

            // Verificar si el producto ya existe en la tabla
            let exists = false;
            let rowIndex = -1;

            $(`#${tableId} tbody tr`).each(function (i) {
                const pid = $(this).find('input[name$=".ProductoID"]').val();
                if (parseInt(pid) === product.id) {
                    exists = true;
                    rowIndex = i;
                    return false; // Romper el bucle
                }
            });

            const subtotal = qty * product.precio;

            if (exists) {
                // Actualizar fila existente
                const $row = $(`#${tableId} tbody tr`).eq(rowIndex);
                const currentQty = parseInt($row.find('.cantidad').val());
                const newQty = currentQty + qty;

                $row.find('.cantidad').val(newQty);
                $row.find('.subtotal').text(App.format.currency(newQty * product.precio));
                $row.find('input[name$=".PrecioTotal"]').val(newQty * product.precio);
            } else {
                // Crear nueva fila
                const rowCount = $(`#${tableId} tbody tr`).length;
                const rowTemplate = this.getProductRowTemplate(rowCount, product, qty, subtotal);
                $(`#${tableId} tbody`).append(rowTemplate);
            }

            // Actualizar totales si se proporciona callback
            if (options.updateTotals && typeof options.updateTotals === 'function') {
                options.updateTotals();
            } else if (options.autoUpdateTotals !== false) {
                App.tables.updateTotals(tableId);
            }

            // Resetear campos del formulario si se solicita
            if (options.resetFields) {
                if (options.codeField) $(options.codeField).val('');
                if (options.nameField) $(options.nameField).val('');
                if (options.priceField) $(options.priceField).val('');
            }

            return true;
        },

        // Plantilla de fila de producto 
        getProductRowTemplate: function (rowIndex, product, quantity, subtotal) {
            return `
                <tr data-index="${rowIndex}">
                    <td>
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].ProductoID" value="${product.id}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].CodigoAlfa" value="${product.codigoAlfa || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].CodigoBarra" value="${product.codigoBarra || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].Marca" value="${product.marca || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].NombreProducto" value="${product.nombre}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].PrecioUnitario" value="${product.precio}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].PrecioTotal" value="${subtotal}" />
                        <input type="hidden" name="ProductosPresupuesto[${rowIndex}].PrecioLista" value="${product.precioLista || 0}" />
                        ${product.codigoAlfa || product.codigoBarra || product.id}
                    </td>
                    <td>${product.nombre}</td>
                    <td><input type="number" name="ProductosPresupuesto[${rowIndex}].Cantidad" value="${quantity}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                    <td>${App.format.currency(product.precio)}</td>
                    <td><span class="subtotal">${App.format.currency(subtotal)}</span></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        }
    };

})(window, jQuery);