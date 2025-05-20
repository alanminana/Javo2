// venta-form.js - Módulo optimizado para formulario de ventas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.ventaForm = {
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
            this.setupCotizacion();
            this.setupCreditoPersonal();
        },

        // Configurar búsqueda de cliente
        setupClienteSearch: function () {
            $('#buscarCliente').off('click').on('click', function () {
                const dni = $('#DniCliente').val();
                App.ventasController.buscarClientePorDNI(dni);
            });

            // Buscar al presionar Enter en DNI
            $('#DniCliente').off('keypress').on('keypress', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#buscarCliente').click();
                    return false;
                }
            });
        },

        // Configurar búsqueda de producto
        setupProductSearch: function () {
            const self = this;
            console.log("DEPURACIÓN: Configurando búsqueda de producto " + new Date().toISOString());

            // Buscar producto
            $('#buscarProducto').off('click').on('click', function (e) {
                console.log("DEPURACIÓN: Click en buscar producto " + new Date().toISOString());
                e.preventDefault();
                const termino = $('#productoCodigo').val();
                if (!termino) {
                    App.notify.warning('Ingrese un código o nombre para buscar');
                    return;
                }

                $.ajax({
                    url: '/Ventas/BuscarProducto',
                    type: 'POST',
                    data: { codigoProducto: termino },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            // Guardar datos del producto con valores seguros
                            self.productoActual = {
                                id: response.data.productoID,
                                codigoAlfa: response.data.codigoAlfa,
                                codigoBarra: response.data.codigoBarra,
                                nombre: response.data.nombreProducto,
                                marca: response.data.marca,
                                precio: parseFloat(response.data.precioUnitario),
                                precioLista: parseFloat(response.data.precioLista)
                            };

                            // Mostrar datos del producto
                            $('#productoNombre').val(self.productoActual.nombre);
                            $('#productoPrecio').val(self.productoActual.precio.toFixed(2));
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
                        App.notify.error('Error al buscar el producto');
                        self.productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
                    }
                });
            });

            // Buscar al presionar Enter - PREVENIR SUBMIT DEL FORMULARIO
            $('#productoCodigo').off('keypress').on('keypress', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#buscarProducto').click();
                    return false;
                }
            });

            // Agregar producto a la tabla
            $('#agregarProducto').off('click').on('click', function (e) {
                e.preventDefault();
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

                // Asegurar que el precio es un número
                const precio = self.productoActual.precio || 0;
                const subtotal = cantidad * precio;

                if (existe) {
                    // Actualizar cantidad
                    const cantidadActual = parseInt($('#productosTable tbody tr').eq(index).find('.cantidad').val());
                    const nuevaCantidad = cantidadActual + cantidad;
                    $('#productosTable tbody tr').eq(index).find('.cantidad').val(nuevaCantidad);

                    // Actualizar subtotal
                    const nuevoSubtotal = nuevaCantidad * precio;
                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(App.format.currency(nuevoSubtotal));
                    $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(nuevoSubtotal.toFixed(2));
                } else {
                    // Crear nueva fila con valores correctamente formateados
                    const rowCount = $('#productosTable tbody tr').length;

                    const newRow = `
                      <tr data-index="${rowCount}">
                          <td>
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].ProductoID" value="${self.productoActual.id}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoAlfa" value="${self.productoActual.codigoAlfa}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoBarra" value="${self.productoActual.codigoBarra}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].Marca" value="${self.productoActual.marca}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].NombreProducto" value="${self.productoActual.nombre}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioUnitario" value="${precio.toFixed(2)}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioTotal" value="${subtotal.toFixed(2)}" />
                              <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioLista" value="${(self.productoActual.precioLista || 0).toFixed(2)}" />
                              ${self.productoActual.codigoAlfa || self.productoActual.codigoBarra}
                          </td>
                          <td>${self.productoActual.nombre}</td>
                          <td><input type="number" name="ProductosPresupuesto[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
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
                $('#productoCodigo, #productoNombre').val('');
                self.productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
            });

            // Eliminar producto
            $(document).off('click', '.eliminar-producto').on('click', '.eliminar-producto', function () {
                $(this).closest('tr').remove();
                self.updateTotals();
                self.reindexRows();
            });

            // Actualizar totales al cambiar cantidad
            $(document).off('change', '.cantidad').on('change', '.cantidad', function () {
                const row = $(this).closest('tr');
                const cantidad = parseInt($(this).val()) || 0;
                const precio = parseFloat(row.find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                row.find('.subtotal').text(App.format.currency(subtotal));
                row.find('input[name$=".PrecioTotal"]').val(subtotal.toFixed(2));

                self.updateTotals();
            });
        },

        // Configurar forma de pago
        setupFormaPago: function () {
            $('#FormaPagoID').off('change').on('change', function () {
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

            // Ejecutar cambio inicial
            $('#FormaPagoID').trigger('change');
        },

        // Configurar crédito personal
        setupCreditoPersonal: function () {
            // Mostrar porcentaje de recargo según cuotas
            $('#Cuotas').off('change').on('change', function () {
                const cuotas = $(this).val();
                if (cuotas) {
                    // Mostrar información de recargo
                    $('#infoRecargo').removeClass('d-none');

                    // Calcular porcentaje basado en número de cuotas
                    let porcentaje = 0;
                    if (cuotas <= 3) {
                        porcentaje = 10;
                    } else if (cuotas <= 6) {
                        porcentaje = 15;
                    } else if (cuotas <= 12) {
                        porcentaje = 20;
                    } else {
                        porcentaje = 25;
                    }

                    $('#porcentajeRecargo').text(porcentaje);
                } else {
                    $('#infoRecargo').addClass('d-none');
                }
            });
        },

        // Configurar opciones de cotización
        setupCotizacion: function () {
            // Crear cotización en lugar de venta
            $('#crearCotizacion').off('click').on('click', function (e) {
                e.preventDefault();

                // Validar que haya cliente y productos
                if (!$('#NombreCliente').val()) {
                    App.notify.warning('Debe seleccionar un cliente para crear una cotización');
                    return;
                }

                if ($('#productosTable tbody tr').length === 0) {
                    App.notify.warning('Debe agregar al menos un producto para crear una cotización');
                    return;
                }

                // Serializar el formulario actual
                const formData = $('#ventaForm').serializeArray();
                $('#cotizacionData').val(JSON.stringify(formData));

                // Enviar formulario de cotización
                $('#cotizacionForm').submit();
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
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal.toFixed(2));
            });

            $('#totalProductos').text(totalProductos);
            $('#totalVenta').text(App.format.currency(totalVenta));

            // Eliminar la entrada anterior y añadir una nueva actualizada
            $('input[name="PrecioTotal"]').remove();

            $('<input>').attr({
                type: 'hidden',
                name: 'PrecioTotal',
                value: totalVenta.toFixed(2)
            }).appendTo('#ventaForm');
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