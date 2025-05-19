// products.js - Módulo para gestión de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.products = {
        init: function () {
            console.log("Módulo de productos inicializado");
        },

        searchByCode: function (url, code, options) {
            options = options || {};

            if (!code) {
                if (options.onError) options.onError("Ingrese un código para buscar");
                return;
            }

            App.ajax.post(url, { codigoProducto: code }, function (response) {
                if (response.success) {
                    const producto = {
                        id: response.data.productoID,
                        codigoAlfa: response.data.codigoAlfa,
                        codigoBarra: response.data.codigoBarra,
                        nombre: response.data.nombreProducto,
                        marca: response.data.marca,
                        precio: response.data.precioUnitario,
                        precioLista: response.data.precioLista
                    };

                    if (options.nameField) $(options.nameField).val(producto.nombre);
                    if (options.priceField) $(options.priceField).val(producto.precio);
                    if (options.quantityField) {
                        $(options.quantityField).val(1);
                        $(options.quantityField).focus();
                    }

                    if (options.onSuccess) options.onSuccess(producto);
                } else {
                    if (options.modalErrorId) $(options.modalErrorId).modal('show');
                    else if (options.onError) options.onError("Producto no encontrado");
                    else App.notify.error("Producto no encontrado");

                    if (options.nameField) $(options.nameField).val('');
                    if (options.priceField) $(options.priceField).val('');

                    if (options.onNotFound) options.onNotFound();
                }
            }, function () {
                if (options.onError) options.onError("Error al buscar producto");
                else App.notify.error("Error al buscar producto");
            });
        },

        addToTable: function (tableId, producto, cantidad, options) {
            options = options || {};

            if (!producto || !producto.id) {
                App.notify.warning("Debe buscar un producto primero");
                return false;
            }

            const cantidadNum = parseInt(cantidad);
            if (isNaN(cantidadNum) || cantidadNum <= 0) {
                App.notify.warning("La cantidad debe ser mayor a cero");
                return false;
            }

            // Verificar si el producto ya está en la tabla
            let existe = false;
            let index = -1;

            $(`#${tableId} tbody tr`).each(function (i) {
                const productoID = $(this).find('input[name$=".ProductoID"]').val();
                if (parseInt(productoID) === producto.id) {
                    existe = true;
                    index = i;
                    return false; // Salir del bucle
                }
            });

            const subtotal = cantidadNum * producto.precio;

            if (existe) {
                // Actualizar cantidad
                const row = $(`#${tableId} tbody tr`).eq(index);
                const cantidadActual = parseInt(row.find('.cantidad').val());
                const nuevaCantidad = cantidadActual + cantidadNum;

                row.find('.cantidad').val(nuevaCantidad);
                row.find('.subtotal').text(App.format.currency(nuevaCantidad * producto.precio));
                row.find('input[name$=".PrecioTotal"]').val(nuevaCantidad * producto.precio);
            } else {
                // Crear nueva fila
                const rowCount = $(`#${tableId} tbody tr`).length;
                const newRow = this.getProductRowTemplate(rowCount, producto, cantidadNum, subtotal);
                $(`#${tableId} tbody`).append(newRow);
            }

            // Actualizar totales
            if (options.updateTotals && typeof options.updateTotals === 'function') {
                options.updateTotals();
            } else if (options.autoUpdateTotals !== false) {
                App.tables.updateTotals(tableId);
            }

            // Limpiar campos
            if (options.resetFields) {
                if (options.codeField) $(options.codeField).val('');
                if (options.nameField) $(options.nameField).val('');
                if (options.priceField) $(options.priceField).val('');
            }

            return true;
        },

        getProductRowTemplate: function (index, producto, cantidad, subtotal) {
            return `
                <tr data-index="${index}">
                    <td>
                        <input type="hidden" name="ProductosPresupuesto[${index}].ProductoID" value="${producto.id}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].CodigoAlfa" value="${producto.codigoAlfa || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].CodigoBarra" value="${producto.codigoBarra || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].Marca" value="${producto.marca || ''}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].NombreProducto" value="${producto.nombre}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].PrecioUnitario" value="${producto.precio}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].PrecioTotal" value="${subtotal}" />
                        <input type="hidden" name="ProductosPresupuesto[${index}].PrecioLista" value="${producto.precioLista || 0}" />
                        ${producto.codigoAlfa || producto.codigoBarra || producto.id}
                    </td>
                    <td>${producto.nombre}</td>
                    <td><input type="number" name="ProductosPresupuesto[${index}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                    <td>${App.format.currency(producto.precio)}</td>
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