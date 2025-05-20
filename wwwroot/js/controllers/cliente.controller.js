// cliente.controller.js - Controlador unificado para clientes
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.clienteController = {
        init: function () {
            this.setupEventHandlers();
            this.setupProvinciasCiudades();
            this.setupCreditoFields();
        },

        setupEventHandlers: function () {
            // Filtrado de clientes
            $('#filtroCliente').on('submit', function (e) {
                // La implementación actual usa el envío de formulario normal
            });

            // Botón para cambiar garante
            $('#cambiarGarante').on('click', function (e) {
                e.preventDefault();
                const clienteId = $('#ClienteID').val();
                window.location.href = `/Clientes/AsignarGarante/${clienteId}`;
            });
        },

        // Configurar manejo de Provincias y Ciudades
        setupProvinciasCiudades: function () {
            const self = this;

            // Cambio de provincia carga ciudades
            $("#provincias, #ProvinciaID").change(function () {
                const provinciaId = $(this).val();
                const ciudadId = $("#ciudades").val() || $("#CiudadID").val();
                const target = $(this).attr('id') === 'provincias' ? 'ciudades' : 'CiudadID';

                self.loadCiudades(provinciaId, target, ciudadId);
            });

            // Inicializar si hay una provincia seleccionada
            if ($("#provincias").val()) {
                $("#provincias").trigger("change");
            }

            if ($("#ProvinciaID").val()) {
                $("#ProvinciaID").trigger("change");
            }
        },

        // Configurar campos de crédito
        setupCreditoFields: function () {
            const toggleCreditoFields = function () {
                const aptoCredito = $("#aptoCredito").is(":checked");
                $(".credito-field").toggleClass("d-none", !aptoCredito);

                // Solo mostrar sección de garante si está habilitado aptoCredito y requiereGarante
                const requiereGarante = $("#requiereGarante").is(":checked");
                $("#garanteInfo").toggleClass("d-none", !(aptoCredito && requiereGarante));
            };

            $("#aptoCredito, #requiereGarante").change(toggleCreditoFields);

            // Inicializar campos de crédito
            toggleCreditoFields();
        },

        // Cargar ciudades para una provincia
        loadCiudades: function (provinciaID, ciudadDropdownId, currentCiudadID) {
            if (!provinciaID) {
                $(`#${ciudadDropdownId}`).empty().append('<option value="">-- Seleccione --</option>');
                return;
            }

            $.ajax({
                url: '/Clientes/GetCiudades',
                type: 'GET',
                data: { provinciaID: provinciaID },
                success: function (ciudades) {
                    var ciudadSelect = $(`#${ciudadDropdownId}`);
                    ciudadSelect.empty();
                    ciudadSelect.append('<option value="">-- Seleccione --</option>');

                    $.each(ciudades, function (i, ciudad) {
                        const option = $('<option></option>').val(ciudad.value).text(ciudad.text);
                        if (currentCiudadID && ciudad.value == currentCiudadID) {
                            option.attr('selected', 'selected');
                        }
                        ciudadSelect.append(option);
                    });
                },
                error: function () {
                    App.notify.error('Error al cargar ciudades');
                }
            });
        },

        // Módulo específico para asignar garante
        asignarGarante: {
            init: function () {
                // Cargar ciudades al cambiar provincia
                $('#ProvinciaID').change(function () {
                    var provinciaID = $(this).val();
                    App.clienteController.loadCiudades(provinciaID, 'CiudadID');
                });

                // Inicializar si ya hay una provincia seleccionada
                if ($('#ProvinciaID').val()) {
                    $('#ProvinciaID').trigger('change');
                }
            }
        },

        // Búsqueda de cliente por DNI (para uso en ventas/cotizaciones)
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

    // Compatibilidad con código existente
    App.clientesController = App.clienteController;
    App.clienteForm = App.clienteController;
    App.asignarGarante = App.clienteController.asignarGarante;

})(window, jQuery);