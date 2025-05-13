
             $(function() {
            // Variables globales para el producto actual
            let productoActual = {
                id: 0,
                codigoAlfa: '',
                codigoBarra: '',
                nombre: '',
                marca: '',
                precio: 0,
                precioLista: 0
            };

            // Formato de moneda para Argentina
            const formatCurrency = new Intl.NumberFormat('es-AR', {
                style: 'currency',
                currency: 'ARS'
            });

            // Búsqueda de cliente por DNI
            $('#buscarCliente').on('click', function() {
                const dni = $('#DniCliente').val();
                if (!dni) {
                    alert('Ingrese un DNI para buscar');
                    return;
                }

                $.ajax({
                    url: '@Url.Action("BuscarClientePorDNI")',
                    type: 'POST',
                    data: { dni },
                    success: function(response) {
                        if (response.success) {
                            // Llenar datos del cliente
                            $('#NombreCliente').val(response.data.nombre);
                            $('#TelefonoCliente').val(response.data.telefono);
                            $('#DomicilioCliente').val(response.data.domicilio);
                            $('#LocalidadCliente').val(response.data.localidad);
                            $('#CelularCliente').val(response.data.celular);
                            $('#LimiteCreditoCliente').val(response.data.limiteCredito);
                            $('#SaldoCliente').val(response.data.saldo);
                            $('#SaldoDisponibleCliente').val(response.data.saldoDisponible);

                            // Ocultar mensaje de error
                            $('#clienteNotFound').addClass('d-none');
                        } else {
                            // Mostrar mensaje de error
                            $('#clienteNotFound').removeClass('d-none');

                            // Limpiar campos
                            $('#NombreCliente, #TelefonoCliente, #DomicilioCliente, #LocalidadCliente, #CelularCliente, #LimiteCreditoCliente, #SaldoCliente, #SaldoDisponibleCliente').val('');
                        }
                    },
                    error: function() {
                        alert('Error al buscar cliente');
                    }
                });
            });

            // Búsqueda de producto
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
                                codigoAlfa: response.data.codigoAlfa,
                                codigoBarra: response.data.codigoBarra,
                                nombre: response.data.nombreProducto,
                                marca: response.data.marca,
                                precio: response.data.precioUnitario,
                                precioLista: response.data.precioLista
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
                            productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
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
                    const subtotal = nuevaCantidad * productoActual.precio;
                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(formatCurrency.format(subtotal));
                } else {
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
                }

                // Actualizar totales
                updateTotals();

                // Limpiar campos
                $('#productoCodigo, #productoNombre').val('');
                productoActual = { id: 0, nombre: '', precio: 0, precioLista: 0 };
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

                // Update hidden field with correct total
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal);
            });

            $('#totalProductos').text(totalProductos);
            $('#totalVenta').text(formatCurrency.format(totalVenta));

            // Add this to update the viewmodel's total
            $('<input>').attr({
                type: 'hidden',
                name: 'PrecioTotal',
                value: totalVenta
            }).appendTo('#cotizacionForm');
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
                }
            });

            // Ejecutar cambio inicial para mostrar campos si ya hay forma de pago seleccionada
            $('#FormaPagoID').trigger('change');

            // Crear cotización en lugar de venta
            $('#crearCotizacion').click(function(e) {
                e.preventDefault();

                // Validar que haya cliente y productos
                if (!$('#NombreCliente').val()) {
                    alert('Debe seleccionar un cliente para crear una cotización');
                    return;
                }

                if ($('#productosTable tbody tr').length === 0) {
                    alert('Debe agregar al menos un producto para crear una cotización');
                    return;
                }

                // Serializar el formulario actual
                const formData = $('#ventaForm').serializeArray();
                $('#cotizacionData').val(JSON.stringify(formData));

                // Enviar formulario de cotización
                $('#cotizacionForm').submit();
            });

            // Inicializar totales
            updateTotals();
        });
