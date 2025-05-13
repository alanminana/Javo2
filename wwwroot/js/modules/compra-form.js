// compra-form.js - Módulo para formulario de compras
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.compraForm = {
        productoActual: {
            id: 0,
            nombre: '',
            precio: 0
        },

        init: function () {
            this.setupProductSearch();
            this.setupFormaPago();
        },

        // Configurar búsqueda de productos
        setupProductSearch: function () {
            const self = this;

            // Colapsar/Expandir sección de búsqueda
            $('#toggleSearchBtn').on('click', function () {
                $('#searchProductSection').toggleClass('d-none');
                $(this).find('i').toggleClass('bi-arrows-expand bi-arrows-collapse');
            });

            // Búsqueda de producto por código
            $('#buscarProducto').on('click', function () {
                const codigo = $('#productoCodigo').val();
                if (!codigo) {
                    App.notify.warning('Ingrese un código para buscar');
                    return;
                }

                App.ajax.post('/Proveedores/BuscarProducto', { codigoProducto: codigo }, function (response) {
                    if (response.success) {
                        // Guardar datos del producto
                        self.productoActual = {
                            id: response.data.productoID,
                            nombre: response.data.nombreProducto,
                            precio: response.data.precioUnitario
                        };

                        // Mostrar datos del producto
                        $('#productoNombre').val(self.productoActual.nombre);
                        $('#productoPrecio').val(self.productoActual.precio);
                        $('#productoCantidad').val(1);
                        $('#productoCantidad').focus();
                    } else {
                        // Mostrar modal de error
                        $('#productoNoEncontradoModal').modal('show');

                        // Limpiar campos
                        self.productoActual = { id: 0, nombre: '', precio: 0 };
                        $('#productoNombre, #productoPrecio').val('');
                    }
                });
            });

            // Abrir modal de búsqueda avanzada
            $('#productoNombre').on('click', function () {
                $('#buscarProductosModal').modal('show');
                $('#modalSearchTerm').focus();
            });

            // Búsqueda en modal
            $('#modalSearchBtn').on('click', function () {
                const term = $('#modalSearchTerm').val();

                App.proveedoresController.searchProductsForPurchase(term, function (data) {
                    // Limpiar resultados anteriores
                    const tbody = $('#modalResultsTable tbody');
                    tbody.empty();

                    if (data.length === 0) {
                        tbody.append('<tr><td colspan="5" class="text-center">No se encontraron productos</td></tr>');
                    } else {
                        // Agregar resultados
                        data.forEach(function (p) {
                            tbody.append(`
                                <tr>
                                    <td>${p.codigo}</td>
                                    <td>${p.name}</td>
                                    <td>${p.marca}</td>
                                    <td>${App.format.currency(p.precio)}</td>
                                    <td class="text-center">
                                        <button type="button" class="btn btn-sm btn-primary select-product"
                                                data-id="${p.id}" data-name="${p.name}" data-precio="${p.precio}">
                                            <i class="bi bi-plus-circle"></i> Seleccionar
                                        </button>
                                    </td>
                                </tr>
                            `);
                        });
                    }
                });
            });

            // Al presionar Enter en el campo de búsqueda del modal
            $('#modalSearchTerm').on('keypress', function (e) {
                if (e.which === 13) {
                    $('#modalSearchBtn').click();
                    return false;
                }
            });

            // Seleccionar producto desde el modal
            $(document).on('click', '.select-product', function () {
                self.productoActual = {
                    id: $(this).data('id'),
                    nombre: $(this).data('name'),
                    precio: $(this).data('precio')
                };

                // Mostrar datos en el formulario
                $('#productoNombre').val(self.productoActual.nombre);
                $('#productoPrecio').val(self.productoActual.precio);
                $('#productoCantidad').val(1);

                // Cerrar modal
                $('#buscarProductosModal').modal('hide');
                $('#productoCantidad').focus();
            });

            // Agregar producto a la tabla
            $('#agregarProducto').on('click', function () {
                if (self.productoActual.id === 0) {
                    App.notify.warning('Debe buscar un producto primero');
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                const precio = parseFloat($('#productoPrecio').val());

                if (isNaN(cantidad) || cantidad <= 0) {
                    App.notify.warning('La cantidad debe ser mayor a cero');
                    return;
                }

                if (isNaN(precio) || precio <= 0) {
                    App.notify.warning('El precio debe ser mayor a cero');
                    return;
                }

                // Verificar si el producto ya está en la tabla
                let existe = false;
                let index = -1;

                $('#productosTable tbody tr').each(function (i) {
                    const productoID = $(this).find('input[name$=".ProductoID"]').val();
                    if (parseInt(productoID) === self.productoActual.id) {
                        existe = true;
                        index = i;
                        return false; // Salir del bucle
                    }
                });

                if (existe) {
                    // Actualizar cantidad
                    const cantidadActual = parseInt($('#productosTable tbody tr').eq(index).find('.cantidad').val());
                    const nuevaCantidad = cantidadActual + cantidad;
                    $('#productosTable tbody tr').eq(index).find('.cantidad').val(nuevaCantidad);

                    // Actualizar subtotal
                    const subtotal = nuevaCantidad * precio;
                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(App.format.currency(subtotal));
                    $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(subtotal);
                } else {
                    // Crear nueva fila
                    const rowCount = $('#productosTable tbody tr').length;
                    const subtotal = cantidad * precio;
                    const newRow = `
                        <tr data-index="${rowCount}">
                            <td>
                                <input type="hidden" name="ProductosCompra[${rowCount}].ProductoID" value="${self.productoActual.id}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].NombreProducto" value="${self.productoActual.nombre}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].PrecioUnitario" value="${precio}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].PrecioTotal" value="${subtotal}" />
                                ${self.productoActual.id}
                            </td>
                            <td>${self.productoActual.nombre}</td>
                            <td><input type="number" name="ProductosCompra[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                            <td>${App.format.currency(precio)}</td>
                            <td><span class="subtotal">${App.format.currency(subtotal)}</span></td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `;

                    $('#productosTable tbody').append(newRow);
                }

                // Actualizar totales
                self.updateTotals();

                // Limpiar campos
                $('#productoCodigo, #productoNombre, #productoPrecio').val('');
                self.productoActual = { id: 0, nombre: '', precio: 0 };
            });

            // Eliminar producto
            $(document).on('click', '.eliminar-producto', function () {
                $(this).closest('tr').remove();
                self.updateTotals();
                self.reindexRows();
            });

            // Actualizar totales al cambiar cantidad
            $(document).on('change', '.cantidad', function () {
                const row = $(this).closest('tr');
                const cantidad = parseInt($(this).val());
                const precio = parseFloat(row.find('input[name$=".PrecioUnitario"]').val());
                const subtotal = cantidad * precio;

                row.find('.subtotal').text(App.format.currency(subtotal));
                row.find('input[name$=".PrecioTotal"]').val(subtotal);

                self.updateTotals();
            });
        },

        // Configurar forma de pago
        setupFormaPago: function () {
            $('#FormaPagoID').change(function () {
                const formaPagoID = parseInt($(this).val());

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor correspondiente
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

            // Ejecutar cambio inicial para mostrar campos si ya hay forma de pago seleccionada
            $('#FormaPagoID').trigger('change');
        },

        // Actualizar totales
        updateTotals: function () {
            let totalProductos = 0;
            let totalCompra = 0;

            $('#productosTable tbody tr').each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                totalProductos += cantidad;
                totalCompra += subtotal;
            });

            $('#totalProductos').text(totalProductos);
            $('#totalCompra').text(App.format.currency(totalCompra));
        },

        // Reindexar filas después de eliminar
        reindexRows: function () {
            $('#productosTable tbody tr').each(function (index) {
                $(this).attr('data-index', index);

                $(this).find('input').each(function () {
                    const name = $(this).attr('name');
                    if (name) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            });
        }
    };

})(window, jQuery);