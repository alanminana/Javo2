// ventas-controller.js - Controlador centralizado para operaciones de ventas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.ventasController = {
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
    };

})(window, jQuery);