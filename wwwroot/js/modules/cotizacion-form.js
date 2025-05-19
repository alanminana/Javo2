// cotizacion-form.js - Módulo para formulario de cotizaciones
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    // Creamos un objeto independiente en lugar de heredar
    App.cotizacionForm = {
        productoActual: {
            id: 0,
            codigoAlfa: '',
            codigoBarra: '',
            nombre: '',
            marca: '',
            precio: 0,
            precioLista: 0
        },

        init: function () {
            this.setupClienteSearch();
            this.setupProductSearch();
            this.setupFormaPago();
            this.setupCotizacionActions();
        },

        // Copiar los métodos necesarios de ventaForm
        setupClienteSearch: function () {
            $('#buscarCliente').on('click', function () {
                const dni = $('#DniCliente').val();
                App.ventasController.buscarClientePorDNI(dni);
            });

            $('#DniCliente').on('keypress', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#buscarCliente').click();
                }
            });
        },

        setupProductSearch: function () {
            const self = this;

            // Búsqueda de producto por código
            $('#buscarProducto').on('click', function () {
                const codigo = $('#productoCodigo').val();
                if (!codigo) {
                    App.notify.warning('Ingrese un código para buscar');
                    return;
                }

                $.ajax({
                    url: '/Cotizaciones/BuscarProducto',
                    type: 'POST',
                    data: { codigoProducto: codigo },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            // Guardar datos del producto
                            self.productoActual = {
                                id: response.data.productoID,
                                codigoAlfa: response.data.codigoAlfa,
                                codigoBarra: response.data.codigoBarra,
                                nombre: response.data.nombreProducto,
                                marca: response.data.marca,
                                precio: response.data.precioUnitario,
                                precioLista: response.data.precioLista
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
                            self.productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
                            $('#productoNombre, #productoPrecio').val('');
                        }
                    },
                    error: function () {
                        console.error('Error en la búsqueda del producto');
                        $('#productoNoEncontradoModal').modal('show');
                    }
                });
            });

            // Agregar producto a la tabla
            $('#agregarProducto').on('click', function () {
                if (self.productoActual.id === 0) {
                    App.notify.warning('Debe buscar un producto primero');
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                if (isNaN(cantidad) || cantidad <= 0) {
                    App.notify.warning('La cantidad debe ser mayor a cero');
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

                const subtotal = cantidad * self.productoActual.precio;

                if (existe) {
                    // Actualizar cantidad
                    const cantidadActual = parseInt($('#productosTable tbody tr').eq(index).find('.cantidad').val());
                    const nuevaCantidad = cantidadActual + cantidad;
                    $('#productosTable tbody tr').eq(index).find('.cantidad').val(nuevaCantidad);

                    // Actualizar subtotal
                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(App.format.currency(nuevaCantidad * self.productoActual.precio));
                    $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(nuevaCantidad * self.productoActual.precio);
                } else {
                    // Crear nueva fila
                    const rowCount = $('#productosTable tbody tr').length;

                    const newRow = `
                        <tr data-index="${rowCount}">
                            <td>
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].ProductoID" value="${self.productoActual.id}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoAlfa" value="${self.productoActual.codigoAlfa || ""}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoBarra" value="${self.productoActual.codigoBarra || ""}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].Marca" value="${self.productoActual.marca || ""}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].NombreProducto" value="${self.productoActual.nombre}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioUnitario" value="${self.productoActual.precio}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioTotal" value="${subtotal}" />
                                <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioLista" value="${self.productoActual.precioLista || 0}" />
                                ${self.productoActual.codigoAlfa || self.productoActual.codigoBarra || self.productoActual.id}
                            </td>
                            <td>${self.productoActual.nombre}</td>
                            <td><input type="number" name="ProductosPresupuesto[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                            <td>${App.format.currency(self.productoActual.precio)}</td>
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
                self.productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
            });

            // Búsqueda con Enter
            $('#productoCodigo').on('keypress', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#buscarProducto').click();
                }
            });

            // Eliminar producto y actualizar cantidades
            $(document).on('click', '.eliminar-producto', function () {
                $(this).closest('tr').remove();
                self.updateTotals();
                self.reindexRows();
            });

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

        setupFormaPago: function () {
            $('#FormaPagoID').change(function () {
                const formaPagoID = parseInt($(this).val());
                console.log('Forma de pago seleccionada:', formaPagoID);

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor correspondiente según ID
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

            // Ejecutar cambio inicial
            $('#FormaPagoID').trigger('change');
        },

        // Funciones específicas para cotizaciones
        setupCotizacionActions: function () {
            // Funcionalidad para convertir cotización a venta
            $('#convertirAVenta').click(function (e) {
                e.preventDefault();

                if (!confirm('¿Está seguro que desea convertir esta cotización en una venta?')) {
                    return;
                }

                const cotizacionId = $(this).data('id');
                $('#convertirForm').submit();
            });
        },

        // Actualizar totales
        updateTotals: function () {
            let totalProductos = 0;
            let totalVenta = 0;

            $('#productosTable tbody tr').each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                totalProductos += cantidad;
                totalVenta += subtotal;

                // Actualizar campo oculto con el total correcto
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal);
            });

            $('#totalProductos').text(totalProductos);
            $('#totalVenta').text(App.format.currency(totalVenta));

            // Actualizar el total del viewmodel
            $('<input>').attr({
                type: 'hidden',
                name: 'PrecioTotal',
                value: totalVenta
            }).appendTo('#cotizacionForm');
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