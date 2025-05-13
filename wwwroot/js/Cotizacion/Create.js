        $(function() {
            // Variables para producto actual
            let productoActual = {
                id: 0,
                codigoAlfa: '',
                codigoBarra: '',
                nombre: '',
                marca: '',
                precio: 0,
                precioLista: 0
            };

            // Formato moneda
            const formatCurrency = new Intl.NumberFormat('es-AR', {
                style: 'currency',
                currency: 'ARS'
            });

            // Búsqueda de producto
            $('#buscarProducto').on('click', function() {
                const codigo = $('#productoCodigo').val();
                if (!codigo) {
                    alert('Ingrese un código o nombre para buscar');
                    return;
                }

                $.ajax({
                    url: '@Url.Action("BuscarProducto")',
                    type: 'POST',
                    data: { codigoProducto: codigo },
                    success: function(response) {
                        if (response.success) {
                            productoActual = {
                                id: response.data.productoID,
                                codigoAlfa: response.data.codigoAlfa,
                                codigoBarra: response.data.codigoBarra,
                                nombre: response.data.nombreProducto,
                                marca: response.data.marca,
                                precio: response.data.precioUnitario,
                                precioLista: response.data.precioLista
                            };
                            $('#productoNombre').val(productoActual.nombre);
                            $('#productoPrecio').val(productoActual.precio);
                            $('#productoCantidad').val(1);
                            $('#productoCantidad').focus();
                        } else {
                            $('#productoNoEncontradoModal').modal('show');
                            productoActual = { id: 0, nombre: '', precio: 0 };
                            $('#productoNombre, #productoPrecio').val('');
                        }
                    },
                    error: function() {
                        alert('Error al buscar producto');
                    }
                });
            });

            // Agregar producto a la tabla
            $('#agregarProducto').on('click', function() {
                if (productoActual.id === 0) {
                    alert('Debe buscar un producto primero');
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                if (isNaN(cantidad) || cantidad <= 0) {
                    alert('La cantidad debe ser mayor a cero');
                    return;
                }

                // Crear nueva fila
                const rowCount = $('#productosTable tbody tr').length;
                const newRow = `
                    <tr data-index="${rowCount}">
                        <td>
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].ProductoID" value="${productoActual.id}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoAlfa" value="${productoActual.codigoAlfa}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].CodigoBarra" value="${productoActual.codigoBarra}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].Marca" value="${productoActual.marca}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].NombreProducto" value="${productoActual.nombre}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioUnitario" value="${productoActual.precio}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioTotal" value="${productoActual.precio * cantidad}" />
                            <input type="hidden" name="ProductosPresupuesto[${rowCount}].PrecioLista" value="${productoActual.precioLista}" />
                            ${productoActual.codigoAlfa || productoActual.codigoBarra}
                        </td>
                        <td>${productoActual.nombre}</td>
                        <td><input type="number" name="ProductosPresupuesto[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                        <td>${formatCurrency.format(productoActual.precio)}</td>
                        <td><span class="subtotal">${formatCurrency.format(productoActual.precio * cantidad)}</span></td>
                        <td class="text-center">
                            <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;

                $('#productosTable tbody').append(newRow);
                updateTotals();

                // Limpiar campos
                $('#productoCodigo, #productoNombre').val('');
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
                let totalVenta = 0;

                $('#productosTable tbody tr').each(function() {
                    const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                    const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                    const subtotal = cantidad * precio;

                    totalProductos += cantidad;
                    totalVenta += subtotal;
                });

                $('#totalProductos').text(totalProductos);
                $('#totalVenta').text(formatCurrency.format(totalVenta));
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

            // Enter key search
            $('#productoCodigo').keypress(function(e) {
                if(e.which === 13) {
                    e.preventDefault();
                    $('#buscarProducto').click();
                }
            });

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
                }
            });

            // Ejecutar cambio inicial para mostrar campos si ya hay forma de pago seleccionada
            $('#FormaPagoID').trigger('change');

            // Inicializar totales
            updateTotals();
        });
