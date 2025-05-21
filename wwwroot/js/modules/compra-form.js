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
            console.log('Inicializando módulo compra-form.js');
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
                    if (typeof App.notify !== 'undefined' && App.notify) {
                        App.notify.warning('Ingrese un código para buscar');
                    } else {
                        alert('Ingrese un código para buscar');
                    }
                    return;
                }

                $.ajax({
                    url: '/Proveedores/BuscarProducto',
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
                    },
                    error: function () {
                        console.error('Error en la búsqueda del producto');
                        // Mostrar modal de error
                        $('#productoNoEncontradoModal').modal('show');
                    }
                });
            });

            // Búsqueda con Enter
            $('#productoCodigo').keypress(function (e) {
                if (e.which === 13) {
                    $('#buscarProducto').click();
                    return false;
                }
            });

            // Búsqueda en modal
            $('#modalSearchBtn').on('click', function () {
                const term = $('#modalSearchTerm').val();
                if (!term || term.length < 2) {
                    if (typeof App.notify !== 'undefined' && App.notify) {
                        App.notify.warning('Ingrese al menos 2 caracteres para buscar');
                    } else {
                        alert('Ingrese al menos 2 caracteres para buscar');
                    }
                    return;
                }

                $.ajax({
                    url: '/Proveedores/SearchProducts',
                    type: 'POST',
                    data: { term: term, forPurchase: true },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (data) {
                        // Limpiar resultados anteriores
                        const tbody = $('#modalResultsTable tbody');
                        tbody.empty();

                        if (!data || data.length === 0) {
                            tbody.append('<tr><td colspan="5" class="text-center">No se encontraron productos</td></tr>');
                        } else {
                            // Agregar resultados
                            data.forEach(function (p) {
                                const formattedPrice = new Intl.NumberFormat('es-AR', {
                                    style: 'currency',
                                    currency: 'ARS'
                                }).format(p.precio);

                                tbody.append(`
                                    <tr>
                                        <td>${p.codigo}</td>
                                        <td>${p.name}</td>
                                        <td>${p.marca}</td>
                                        <td>${formattedPrice}</td>
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
                    },
                    error: function () {
                        console.error('Error en la búsqueda de productos');
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
                    if (typeof App.notify !== 'undefined' && App.notify) {
                        App.notify.warning('Debe buscar un producto primero');
                    } else {
                        alert('Debe buscar un producto primero');
                    }
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                const precio = parseFloat($('#productoPrecio').val());

                if (isNaN(cantidad) || cantidad <= 0) {
                    if (typeof App.notify !== 'undefined' && App.notify) {
                        App.notify.warning('La cantidad debe ser mayor a cero');
                    } else {
                        alert('La cantidad debe ser mayor a cero');
                    }
                    return;
                }

                if (isNaN(precio) || precio <= 0) {
                    if (typeof App.notify !== 'undefined' && App.notify) {
                        App.notify.warning('El precio debe ser mayor a cero');
                    } else {
                        alert('El precio debe ser mayor a cero');
                    }
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
                    const subtotalFormatted = new Intl.NumberFormat('es-AR', {
                        style: 'currency',
                        currency: 'ARS'
                    }).format(subtotal);

                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(subtotalFormatted);
                    $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(subtotal);
                } else {
                    // Crear nueva fila
                    const rowCount = $('#productosTable tbody tr').length;
                    const subtotal = cantidad * precio;
                    const subtotalFormatted = new Intl.NumberFormat('es-AR', {
                        style: 'currency',
                        currency: 'ARS'
                    }).format(subtotal);

                    const precioFormatted = new Intl.NumberFormat('es-AR', {
                        style: 'currency',
                        currency: 'ARS'
                    }).format(precio);

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
                            <td>${precioFormatted}</td>
                            <td><span class="subtotal">${subtotalFormatted}</span></td>
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

                const subtotalFormatted = new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: 'ARS'
                }).format(subtotal);

                row.find('.subtotal').text(subtotalFormatted);
                row.find('input[name$=".PrecioTotal"]').val(subtotal);

                self.updateTotals();
            });
        },

        // Configurar forma de pago
        setupFormaPago: function () {
            console.log('Inicializando setupFormaPago');

            $('#FormaPagoID').change(function () {
                const formaPagoID = parseInt($(this).val());
                console.log('Forma de pago seleccionada:', formaPagoID);

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor correspondiente
                switch (formaPagoID) {
                    case 2: // Tarjeta de Crédito
                        console.log('Mostrando contenedor: tarjetaCreditoContainer');
                        $('#tarjetaCreditoContainer').removeClass('d-none');
                        break;
                    case 3: // Tarjeta de Débito
                        console.log('Mostrando contenedor: tarjetaDebitoContainer');
                        $('#tarjetaDebitoContainer').removeClass('d-none');
                        break;
                    case 4: // Transferencia
                        console.log('Mostrando contenedor: transferenciaContainer');
                        $('#transferenciaContainer').removeClass('d-none');
                        break;
                    case 5: // Pago Virtual
                        console.log('Mostrando contenedor: pagoVirtualContainer');
                        $('#pagoVirtualContainer').removeClass('d-none');
                        break;
                    case 6: // Crédito Personal
                        console.log('Mostrando contenedor: creditoPersonalContainer');
                        $('#creditoPersonalContainer').removeClass('d-none');
                        break;
                    case 7: // Cheque
                        console.log('Mostrando contenedor: chequeContainer');
                        $('#chequeContainer').removeClass('d-none');
                        break;
                }
            });

            // Ejecutar cambio inicial para mostrar campos si ya hay forma de pago seleccionada
            console.log('Ejecutando trigger inicial para FormaPagoID');
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

            const totalCompraFormatted = new Intl.NumberFormat('es-AR', {
                style: 'currency',
                currency: 'ARS'
            }).format(totalCompra);

            $('#totalCompra').text(totalCompraFormatted);
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