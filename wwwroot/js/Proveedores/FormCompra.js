    <script defer>
        (function($) {
            $(function() {
                // Variables globales para el producto actual
                let productoActual = {
                    id: 0,
                    nombre: '',
                    precio: 0
                };

                // Formato de moneda para Argentina
                const formatCurrency = new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: 'ARS'
                });

                // Colapsar/Expandir sección de búsqueda
                $('#toggleSearchBtn').on('click', function() {
                    $('#searchProductSection').toggleClass('d-none');
                    $(this).find('i').toggleClass('bi-arrows-expand bi-arrows-collapse');
                });

                // Búsqueda de producto por código
                $('#buscarProducto').on('click', function() {
                    const codigo = $('#productoCodigo').val();
                    if (!codigo) {
                        alert('Ingrese un código para buscar');
                        return;
                    }

                    $.ajax({
                        url: '@Url.Action("BuscarProducto")',
                        type: 'POST',
                        data: { codigoProducto: codigo },
                        success: function(response) {
                            if (response.success) {
                                // Guardar datos del producto
                                productoActual = {
                                    id: response.data.productoID,
                                    nombre: response.data.nombreProducto,
                                    precio: response.data.precioUnitario
                                };

                                // Mostrar datos del producto
                                $('#productoNombre').val(productoActual.nombre);
                                $('#productoPrecio').val(productoActual.precio);
                                $('#productoCantidad').val(1);
                                $('#productoCantidad').focus();
                            } else {
                                // Mostrar modal de error
                                $('#productoNoEncontradoModal').modal('show');

                                // Limpiar campos
                                productoActual = { id: 0, nombre: '', precio: 0 };
                                $('#productoNombre, #productoPrecio').val('');
                            }
                        },
                        error: function() {
                            alert('Error al buscar producto');
                        }
                    });
                });

                // Abrir modal de búsqueda avanzada
                $('#productoNombre').on('click', function() {
                    $('#buscarProductosModal').modal('show');
                    $('#modalSearchTerm').focus();
                });

                // Búsqueda en modal
                $('#modalSearchBtn').on('click', function() {
                    const term = $('#modalSearchTerm').val();
                    if (!term || term.length < 2) {
                        alert('Ingrese al menos 2 caracteres para buscar');
                        return;
                    }

                    $.ajax({
                        url: '@Url.Action("SearchProductsForPurchase", "Proveedores")',
                        type: 'POST',
                        data: { term: term },
                        success: function(data) {
                            // Limpiar resultados anteriores
                            const tbody = $('#modalResultsTable tbody');
                            tbody.empty();

                            if (data.length === 0) {
                                tbody.append('<tr><td colspan="5" class="text-center">No se encontraron productos</td></tr>');
                            } else {
                                // Agregar resultados
                                data.forEach(function(p) {
                                    tbody.append(`
                                        <tr>
                                            <td>${p.codigo}</td>
                                            <td>${p.name}</td>
                                            <td>${p.marca}</td>
                                            <td>${formatCurrency.format(p.precio)}</td>
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
                        error: function() {
                            alert('Error al buscar productos');
                        }
                    });
                });

                // Al presionar Enter en el campo de búsqueda del modal
                $('#modalSearchTerm').on('keypress', function(e) {
                    if (e.which === 13) {
                        $('#modalSearchBtn').click();
                        return false;
                    }
                });

                // Seleccionar producto desde el modal
                $(document).on('click', '.select-product', function() {
                    productoActual = {
                        id: $(this).data('id'),
                        nombre: $(this).data('name'),
                        precio: $(this).data('precio')
                    };

                    // Mostrar datos en el formulario
                    $('#productoNombre').val(productoActual.nombre);
                    $('#productoPrecio').val(productoActual.precio);
                    $('#productoCantidad').val(1);

                    // Cerrar modal
                    $('#buscarProductosModal').modal('hide');
                    $('#productoCantidad').focus();
                });

                // Agregar producto a la tabla
                $('#agregarProducto').on('click', function() {
                    if (productoActual.id === 0) {
                        alert('Debe buscar un producto primero');
                        return;
                    }

                    const cantidad = parseInt($('#productoCantidad').val());
                    const precio = parseFloat($('#productoPrecio').val());

                    if (isNaN(cantidad) || cantidad <= 0) {
                        alert('La cantidad debe ser mayor a cero');
                        return;
                    }

                    if (isNaN(precio) || precio <= 0) {
                        alert('El precio debe ser mayor a cero');
                        return;
                    }

                    // Verificar si el producto ya está en la tabla
                    let existe = false;
                    let index = -1;

                    $('#productosTable tbody tr').each(function(i) {
                        const productoID = $(this).find('input[name$=".ProductoID"]').val();
                        if (parseInt(productoID) === productoActual.id) {
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
                        $('#productosTable tbody tr').eq(index).find('.subtotal').text(formatCurrency.format(subtotal));
                        $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(subtotal);
                    } else {
                        // Crear nueva fila
                        const rowCount = $('#productosTable tbody tr').length;
                        const subtotal = cantidad * precio;
                        const newRow = `
                            <tr data-index="${rowCount}">
                                <td>
                                    <input type="hidden" name="ProductosCompra[${rowCount}].ProductoID" value="${productoActual.id}" />
                                    <input type="hidden" name="ProductosCompra[${rowCount}].NombreProducto" value="${productoActual.nombre}" />
                                    <input type="hidden" name="ProductosCompra[${rowCount}].PrecioUnitario" value="${precio}" />
                                    <input type="hidden" name="ProductosCompra[${rowCount}].PrecioTotal" value="${subtotal}" />
                                    ${productoActual.id}
                                </td>
                                <td>${productoActual.nombre}</td>
                                <td><input type="number" name="ProductosCompra[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                                <td>${formatCurrency.format(precio)}</td>
                                <td><span class="subtotal">${formatCurrency.format(subtotal)}</span></td>
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
                    updateTotals();

                    // Limpiar campos
                    $('#productoCodigo, #productoNombre, #productoPrecio').val('');
                    productoActual = { id: 0, nombre: '', precio: 0 };
                });

                // Eliminar producto
                $(document).on('click', '.eliminar-producto', function() {
                    $(this).closest('tr').remove();
                    updateTotals();
                    reindexRows();
                });

                // Actualizar totales al cambiar cantidad
                $(document).on('change', '.cantidad', function() {
                    const row = $(this).closest('tr');
                    const cantidad = parseInt($(this).val());
                    const precio = parseFloat(row.find('input[name$=".PrecioUnitario"]').val());
                    const subtotal = cantidad * precio;

                    row.find('.subtotal').text(formatCurrency.format(subtotal));
                    row.find('input[name$=".PrecioTotal"]').val(subtotal);

                    updateTotals();
                });

                // Función para actualizar totales
                function updateTotals() {
                    let totalProductos = 0;
                    let totalCompra = 0;

                    $('#productosTable tbody tr').each(function() {
                        const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                        const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                        const subtotal = cantidad * precio;

                        totalProductos += cantidad;
                        totalCompra += subtotal;
                    });

                    $('#totalProductos').text(totalProductos);
                    $('#totalCompra').text(formatCurrency.format(totalCompra));
                }

                // Función para reindexar filas después de eliminar
                function reindexRows() {
                    $('#productosTable tbody tr').each(function(index) {
                        $(this).attr('data-index', index);

                        $(this).find('input').each(function() {
                            const name = $(this).attr('name');
                            if (name) {
                                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                                $(this).attr('name', newName);
                            }
                        });
                    });
                }

                // Mostrar/ocultar campos según forma de pago
                $('#FormaPagoID').change(function() {
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

                // Inicializar totales
                updateTotals();
            });
        })(jQuery);
    </script>
