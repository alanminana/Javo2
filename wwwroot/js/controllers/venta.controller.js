// venta.controller.js - Controlador unificado para ventas y entregas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.ventaController = {
        selectedId: null,

        init: function () {
            this.setupActionHandlers();
            this.setupEntregaHandlers();
        },

        // Configurar manejadores de acciones
        setupActionHandlers: function () {
            const self = this;

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

            // Marcar como entregada (confirmación en modal)
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

        // Configurar manejadores de eventos para entregas
        setupEntregaHandlers: function () {
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
            if (document.getElementById('confirmDeliveryBtn')) {
                document.getElementById('confirmDeliveryBtn').addEventListener('click', () => {
                    self.marcarEntregada();
                });
            }
        },

        // Marcar venta como entregada
        marcarEntregada: function () {
            if (!this.selectedId) return;

            fetch('/Ventas/MarcarEntregada', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: this.selectedId })
            })
                .then(res => res.json())
                .then(resp => {
                    if (resp.success) {
                        location.reload();
                    } else {
                        App.notify.error(resp.message || 'Error al procesar entrega');
                    }
                })
                .catch(() => {
                    App.notify.error('Error de red al procesar la solicitud');
                });
        },

        // Búsqueda de cliente por DNI (para formularios de venta)
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
        },

        // Submódulo para devoluciones y garantías
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
                         <td>${App.format.currency(item.precioUnitario)}</td>
                         <td>${App.format.currency(item.subtotal)}</td>
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
            }
        }
    };

    // Compatibilidad con código existente
    App.ventasController = App.ventaController;
    App.entregaProductos = App.ventaController;
    App.devolucionForm = App.ventaController.devolucionForm;
    App.devolucionUtils = {
        init: function () {
            // Ocultar alertas después de 5 segundos
            setTimeout(function () {
                $('.alert').alert('close');
            }, 5000);
        }
    };

})(window, jQuery);