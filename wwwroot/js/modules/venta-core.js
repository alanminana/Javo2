// venta-core.js - Módulo central unificado para todo el proceso de ventas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.ventaCore = {
        // Variables compartidas
        productoActual: {
            id: 0,
            codigoAlfa: '',
            codigoBarra: '',
            nombre: '',
            marca: '',
            precio: 0,
            precioLista: 0
        },

        selectedId: null, // Para operaciones que requieren seleccionar un ID

        // INICIALIZACIÓN Y DETECCIÓN DE CONTEXTO

        init: function () {
            console.log("Inicializando módulo central de ventas");

            // Detectar en qué página estamos
            const isVentaForm = $('#ventaForm').length > 0;
            const isCotizacionForm = $('#cotizacionForm').length > 0;
            const isEntregaProductos = $('.mark-delivered').length > 0;
            const isDevolucionForm = $('#devolucionForm').length > 0;
            const isVentasList = $('#ventasTable').length > 0 && !isVentaForm;

            // Inicializar funcionalidad base siempre
            this.setupActionHandlers();

            // Inicializar componentes según la página
            if (isVentaForm || isCotizacionForm) {
                this.setupClienteSearch();
                this.setupProductSearch();
                this.setupFormaPago();
            }

            if (isVentaForm) {
                this.setupCreditoPersonal();
                this.setupCotizacion();
            }

            if (isCotizacionForm) {
                this.setupCotizacionActions();
            }

            if (isEntregaProductos) {
                this.setupEntregaEventHandlers();
            }

            if (isDevolucionForm) {
                this.devolucionForm.init();
            }

            if (isVentasList) {
                // Controlador para listados y acciones en general
                this.controller.init();
            }
        },

        // CONTROLADOR CENTRAL (ventas-controller.js)

        controller: {
            init: function () {
                this.setupActionHandlers();
            },

            // Configurar manejadores de acciones
            setupActionHandlers: function () {
                // Autorizar venta
                $(document).on('click', '.autorizar-venta', function (e) {
                    e.preventDefault();
                    const ventaId = $(this).data('id');
                    const $row = $(this).closest('tr');

                    if (confirm('¿Está seguro que desea autorizar esta venta?')) {
                        App.ajax.post('/Ventas/Autorizar', { id: ventaId }, function (response) {
                            if (response.success) {
                                $row.fadeOut(400, function () {
                                    $(this).remove();
                                    if ($('table tbody tr').length === 0) {
                                        location.reload();
                                    } else {
                                        App.notify.success("Venta autorizada correctamente");
                                    }
                                });
                            } else {
                                App.notify.error(response.message || 'Error al autorizar la venta.');
                            }
                        });
                    }
                });

                // Rechazar venta
                $(document).on('click', '.rechazar-venta', function (e) {
                    e.preventDefault();
                    const ventaId = $(this).data('id');
                    const $row = $(this).closest('tr');

                    if (confirm('¿Está seguro que desea rechazar esta venta?')) {
                        App.ajax.post('/Ventas/Rechazar', { id: ventaId }, function (response) {
                            if (response.success) {
                                $row.fadeOut(400, function () {
                                    $(this).remove();
                                    if ($('table tbody tr').length === 0) {
                                        location.reload();
                                    } else {
                                        App.notify.success("Venta rechazada correctamente");
                                    }
                                });
                            } else {
                                App.notify.error(response.message || 'Error al rechazar la venta.');
                            }
                        });
                    }
                });

                // Marcar como entregada (botón en tabla)
                $(document).on('click', '.marcar-entregada', function () {
                    const ventaId = $(this).data('id');
                    $('#confirmarEntregaModal').modal('show');
                    $('#confirmarEntrega').data('id', ventaId);
                });

                // Marcar como entregada (confirmación)
                $(document).on('click', '#confirmarEntrega', function () {
                    const ventaId = $(this).data('id');

                    $('#confirmarEntregaModal').modal('hide');

                    App.ajax.post('/Ventas/MarcarEntregada', { id: ventaId }, function (response) {
                        if (response.success) {
                            App.notify.success('Venta marcada como entregada exitosamente');
                            $(`button[data-id="${ventaId}"]`).closest('tr').fadeOut(400, function () {
                                $(this).remove();
                                if ($('.table tbody tr').length === 0) {
                                    location.reload();
                                }
                            });
                        } else {
                            App.notify.error(response.message || 'Error al marcar la venta como entregada');
                        }
                    });
                });
            },

            // Búsqueda de cliente por DNI
            buscarClientePorDNI: function (dni, callbacks) {
                if (!dni) {
                    App.notify.warning('Ingrese un DNI para buscar');
                    return;
                }

                App.ajax.post('/Ventas/BuscarClientePorDNI', { dni }, function (response) {
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

                        if (callbacks && callbacks.onSuccess) {
                            callbacks.onSuccess(response.data);
                        }
                    } else {
                        // Mostrar mensaje de error
                        $('#clienteNotFound').removeClass('d-none');

                        // Limpiar campos
                        $('#NombreCliente, #TelefonoCliente, #DomicilioCliente, #LocalidadCliente, #CelularCliente, #LimiteCreditoCliente, #SaldoCliente, #SaldoDisponibleCliente').val('');

                        if (callbacks && callbacks.onError) {
                            callbacks.onError(response.message);
                        }
                    }
                }, function () {
                    App.notify.error('Error al buscar cliente');

                    if (callbacks && callbacks.onError) {
                        callbacks.onError('Error al buscar cliente');
                    }
                });
            }
        },

        // FORMULARIO DE VENTAS Y COTIZACIONES

        // BÚSQUEDA DE CLIENTES

        setupClienteSearch: function () {
            const self = this;

            if (!$('#buscarCliente').length) return;

            $('#buscarCliente').off('click').on('click', function () {
                const dni = $('#DniCliente').val();
                self.controller.buscarClientePorDNI(dni);
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

        // BÚSQUEDA Y MANIPULACIÓN DE PRODUCTOS

        setupProductSearch: function () {
            const self = this;
            if (!$('#buscarProducto').length) return;

            // Buscar producto
            $('#buscarProducto').off('click').on('click', function (e) {
                e.preventDefault();
                const termino = $('#productoCodigo').val();
                if (!termino) {
                    App.notify?.warning('Ingrese un código o nombre para buscar');
                    return;
                }

                // Determinar URL según la página
                const url = $('#cotizacionForm').length ?
                    '/Cotizaciones/BuscarProducto' : '/Ventas/BuscarProducto';

                $.ajax({
                    url: url,
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
                                codigoAlfa: response.data.codigoAlfa || '',
                                codigoBarra: response.data.codigoBarra || '',
                                nombre: response.data.nombreProducto || '',
                                marca: response.data.marca || '',
                                precio: parseFloat(response.data.precioUnitario) || 0,
                                precioLista: parseFloat(response.data.precioLista) || 0
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
                        App.notify?.error('Error al buscar el producto');
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
                    App.notify?.warning('Debe buscar un producto primero');
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                if (isNaN(cantidad) || cantidad <= 0) {
                    App.notify?.warning('La cantidad debe ser mayor a cero');
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

                // Determinar nombre del array según el formulario
                const namePrefix = $('#cotizacionForm').length ? 'ProductosPresupuesto' : 'ProductosVenta';

                if (existe) {
                    // Actualizar cantidad
                    const cantidadActual = parseInt($('#productosTable tbody tr').eq(index).find('.cantidad').val());
                    const nuevaCantidad = cantidadActual + cantidad;
                    $('#productosTable tbody tr').eq(index).find('.cantidad').val(nuevaCantidad);

                    // Actualizar subtotal
                    const nuevoSubtotal = nuevaCantidad * precio;
                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(App.format.currency(nuevoSubtotal));
                    $('#productosTable tbody tr').eq(index).find(`input[name$=".PrecioTotal"]`).val(nuevoSubtotal.toFixed(2));
                } else {
                    // Crear nueva fila con valores correctamente formateados
                    const rowCount = $('#productosTable tbody tr').length;

                    const newRow = `
                      <tr data-index="${rowCount}">
                          <td>
                              <input type="hidden" name="${namePrefix}[${rowCount}].ProductoID" value="${self.productoActual.id}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].CodigoAlfa" value="${self.productoActual.codigoAlfa}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].CodigoBarra" value="${self.productoActual.codigoBarra}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].Marca" value="${self.productoActual.marca}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].NombreProducto" value="${self.productoActual.nombre}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].PrecioUnitario" value="${precio.toFixed(2)}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].PrecioTotal" value="${subtotal.toFixed(2)}" />
                              <input type="hidden" name="${namePrefix}[${rowCount}].PrecioLista" value="${(self.productoActual.precioLista || 0).toFixed(2)}" />
                              ${self.productoActual.codigoAlfa || self.productoActual.codigoBarra || self.productoActual.id}
                          </td>
                          <td>${self.productoActual.nombre}</td>
                          <td><input type="number" name="${namePrefix}[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                          <td>${App.format?.currency ? App.format.currency(precio) : precio.toFixed(2)}</td>
                          <td><span class="subtotal">${App.format?.currency ? App.format.currency(subtotal) : subtotal.toFixed(2)}</span></td>
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

                row.find('.subtotal').text(App.format?.currency ? App.format.currency(subtotal) : subtotal.toFixed(2));
                row.find('input[name$=".PrecioTotal"]').val(subtotal.toFixed(2));

                self.updateTotals();
            });
        },

        // Configurar forma de pago
        setupFormaPago: function () {
            if (!$('#FormaPagoID').length) return;

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
            if (!$('#Cuotas').length) return;

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
            if (!$('#crearCotizacion').length) return;

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

        // Configurar acciones específicas de cotizaciones
        setupCotizacionActions: function () {
            if (!$('#convertirAVenta').length) return;

            // Funcionalidad para convertir cotización a venta
            const convertirButton = document.getElementById('convertirAVenta');
            if (convertirButton) {
                convertirButton.addEventListener('click', function (e) {
                    e.preventDefault();

                    if (!confirm('¿Está seguro que desea convertir esta cotización en una venta?')) {
                        return;
                    }

                    const cotizacionId = this.getAttribute('data-id');
                    document.getElementById('convertirForm')?.submit();
                });
            }
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
            $('#totalVenta').text(App.format?.currency ? App.format.currency(totalVenta) : totalVenta.toFixed(2));

            // Determinar ID del formulario según la página
            const formId = $('#cotizacionForm').length ? 'cotizacionForm' : 'ventaForm';

            // Eliminar la entrada anterior y añadir una nueva actualizada
            $('input[name="PrecioTotal"]').remove();

            // Solo agregar si existe el formulario
            if ($(`#${formId}`).length) {
                $('<input>').attr({
                    type: 'hidden',
                    name: 'PrecioTotal',
                    value: totalVenta.toFixed(2)
                }).appendTo(`#${formId}`);
            }
        },

        // Reindexar filas después de eliminar
        reindexRows: function () {
            // Determinar nombre del array según el formulario
            const namePrefix = $('#cotizacionForm').length ? 'ProductosPresupuesto' : 'ProductosVenta';

            $('#productosTable tbody tr').each(function (index) {
                $(this).attr('data-index', index);

                $(this).find('input').each(function () {
                    const name = $(this).attr('name');
                    if (name && name.includes('[')) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            });
        },

        // ENTREGA DE PRODUCTOS

        setupEntregaEventHandlers: function () {
            const self = this;

            // Botones de marcar entregado
            document.querySelectorAll('.mark-delivered').forEach(btn => {
                btn.addEventListener('click', () => {
                    self.selectedId = btn.dataset.id;
                    var modal = new bootstrap.Modal(document.getElementById('confirmDeliveryModal'));
                    modal.show();
                });
            });

            // Confirmar entrega
            document.getElementById('confirmDeliveryBtn')?.addEventListener('click', () => {
                self.marcarEntregada();
            });
        },

        // Marcar venta como entregada
        marcarEntregada: function () {
            if (!this.selectedId) return;

            fetch('/Ventas/MarcarEntregada', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ id: this.selectedId })
            })
                .then(res => res.json())
                .then(resp => {
                    if (resp.success) {
                        location.reload();
                    } else {
                        App.notify?.error(resp.message || 'Error al procesar entrega');
                    }
                })
                .catch(() => {
                    App.notify?.error('Error de red al procesar la solicitud');
                });
        },

        // DEVOLUCIONES Y GARANTÍAS

        devolucionForm: {
            init: function () {
                this.setupEventHandlers();
            },

            setupEventHandlers: function () {
                const self = this;

                // Búsqueda de venta
                $("#btnBuscarVenta").click(function () {
                    var numeroFactura = $("#buscarVenta").val();
                    if (!numeroFactura) {
                        self.showError('Ingrese un número de factura válido');
                        return;
                    }

                    $("#errorBusqueda").addClass("d-none");

                    $.ajax({
                        url: '/DevolucionGarantia/BuscarVenta',
                        type: 'POST',
                        data: { numeroFactura: numeroFactura },
                        success: function (result) {
                            if (result.success) {
                                $("#formFields").removeClass("d-none");
                                $("#paso1").hide();

                                self.renderProductTable(result.items);
                            } else {
                                self.showError(result.message);
                            }
                        },
                        error: function () {
                            self.showError('Error al buscar la venta');
                        }
                    });
                });

                // Seleccionar todos
                $("#selectAllProducts").change(function () {
                    $(".product-check").prop('checked', $(this).prop('checked'));
                });

                // Cambio de TipoCaso
                $("#TipoCaso").change(function () {
                    if ($(this).val() == '1') { // Assuming 1 is the ID for Cambio
                        self.setupCambioProductos();
                        $("#seccionCambio").removeClass("d-none");
                    } else {
                        $("#seccionCambio").addClass("d-none");
                    }
                });

                // Búsqueda por enter
                $("#buscarVenta").keypress(function (e) {
                    if (e.which === 13) {
                        e.preventDefault();
                        $("#btnBuscarVenta").click();
                    }
                });
            },

            // Renderizar tabla de productos de venta
            renderProductTable: function (items) {
                var tbody = $("#tablaProductos tbody").empty();

                items.forEach((item, i) => {
                    tbody.append(
                        `<tr data-id="${item.productoID}">
                          <td>
                            <input type="checkbox" class="product-check" name="Items[${i}].Seleccionado" />
                            <input type="hidden" name="Items[${i}].ProductoID" value="${item.productoID}" />
                            <input type="hidden" name="Items[${i}].NombreProducto" value="${item.nombreProducto}" />
                            <input type="hidden" name="Items[${i}].PrecioUnitario" value="${item.precioUnitario}" />
                          </td>
                          <td>${item.nombreProducto}</td>
                          <td><input type="number" class="form-control" name="Items[${i}].Cantidad" value="${item.cantidad}" min="1"/></td>
                          <td>${this.formatCurrency(item.precioUnitario)}</td>
                          <td>${this.formatCurrency(item.subtotal)}</td>
                          <td>
                            <select class="form-select" name="Items[${i}].EstadoProducto">
                              <option>Funcional</option>
                              <option>Defectuoso</option>
                              <option>Dañado</option>
                              <option>Incompleto</option>
                              <option>Sin usar</option>
                            </select>
                          </td>
                          <td><input type="checkbox" name="Items[${i}].ProductoDanado" /></td>
                        </tr>`);
                });
            },

            // Configurar tabla de cambios de productos
            setupCambioProductos: function () {
                var tabla = $("#tablaCambios tbody").empty();

                $(".product-check:checked").each(function (i) {
                    var row = $(this).closest('tr'),
                        id = row.data('id'),
                        nm = row.find('td:eq(1)').text(),
                        qt = row.find('input[name$=".Cantidad"]').val();

                    tabla.append(
                        `<tr>
                          <td>${nm}<input type="hidden" name="CambiosProducto[${i}].ProductoOriginalID" value="${id}" /></td>
                          <td>
                            <input type="text" class="form-control producto-nuevo-buscar" placeholder="Buscar producto..." />
                            <input type="hidden" name="CambiosProducto[${i}].ProductoNuevoID" class="producto-nuevo-id" />
                            <input type="hidden" name="CambiosProducto[${i}].NombreProductoNuevo" class="producto-nuevo-nombre" />
                          </td>
                          <td><input type="number" class="form-control" name="CambiosProducto[${i}].Cantidad" value="${qt}" min="1"/></td>
                          <td><input type="number" class="form-control" name="CambiosProducto[${i}].DiferenciaPrecio" step="0.01"/></td>
                          <td>${nm}<input type="hidden" name="CambiosProducto[${i}].ProductoOriginalID" value="${id}" /></td>
                          <td>
                            <input type="text" class="form-control producto-nuevo-buscar" placeholder="Buscar producto..." />
                            <input type="hidden" name="CambiosProducto[${i}].ProductoNuevoID" class="producto-nuevo-id" />
                            <input type="hidden" name="CambiosProducto[${i}].NombreProductoNuevo" class="producto-nuevo-nombre" />
                          </td>
                          <td><input type="number" class="form-control" name="CambiosProducto[${i}].Cantidad" value="${qt}" min="1"/></td>
                          <td><input type="number" class="form-control" name="CambiosProducto[${i}].DiferenciaPrecio" step="0.01"/></td>
                          <td><button type="button" class="btn btn-sm btn-outline-danger eliminar-cambio">✕</button></td>
                        </tr>`);
                });

                $(".producto-nuevo-buscar").autocomplete({
                    source: function (r, s) {
                        $.getJSON('/DevolucionGarantia/BuscarProductos', { term: r.term }, s);
                    },
                    minLength: 2,
                    select: function (e, ui) {
                        $(this).val(ui.item.label);
                        $(this).siblings('.producto-nuevo-id').val(ui.item.id);
                        $(this).siblings('.producto-nuevo-nombre').val(ui.item.label);
                        return false;
                    }
                });

                // Manejador para eliminar cambio
                $(document).on('click', '.eliminar-cambio', function () {
                    $(this).closest('tr').remove();
                });
            },

            // Mostrar error
            showError: function (message) {
                $("#errorBusqueda").text(message).removeClass('d-none');
            },

            // Formatear moneda
            formatCurrency: function (value) {
                return App.format?.currency ? App.format.currency(value) : new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: 'ARS'
                }).format(value);
            }
        },

        // MANEJADORES DE ACCIONES COMPARTIDOS

        setupActionHandlers: function () {
            // Botones y acciones compartidas entre todas las vistas relacionadas con ventas

            // Por ejemplo, todos los botones para marcar acciones
            $(document).off('click', '.action-button').on('click', '.action-button', function () {
                // Código compartido para botones de acción
                const action = $(this).data('action');
                const id = $(this).data('id');

                if (action && id) {
                    // Procesar acciones comunes
                    console.log(`Acción ${action} para ID ${id}`);
                }
            });
        }
    };

    // Alias para compatibilidad con código existente
    App.ventaForm = App.ventaCore;
    App.cotizacionForm = App.ventaCore;
    App.devolucionForm = App.ventaCore.devolucionForm;
    App.entregaProductos = {
        init: function () { App.ventaCore.setupEntregaEventHandlers(); },
        marcarEntregada: function () { App.ventaCore.marcarEntregada(); },
        selectedId: App.ventaCore.selectedId
    };
    App.ventasController = App.ventaCore.controller;

})(window, jQuery);