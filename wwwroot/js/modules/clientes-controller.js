// clientes-controller.js - Módulo unificado para clientes
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.clientesController = {
        // INICIALIZACIÓN

        init: function () {
            console.log("Inicializando módulo unificado de clientes");

            // Detectar contexto
            const isClientesList = $('#filtroCliente').length > 0;
            const isClienteForm = $('#provincias').length > 0;
            const isAsignarGarante = $('#asignarGaranteForm').length > 0;

            // Inicializar componentes según el contexto
            this.setupEventHandlers();

            if (isClienteForm) {
                this.setupProvinciasCiudades();
                this.setupCreditoFields();
            }

            if (isAsignarGarante) {
                this.setupAsignarGarante();
            }
        },

        // CONTROLADOR GENERAL

        setupEventHandlers: function () {
            // Filtrado de clientes
            $('#filtroCliente').on('submit', function (e) {
                // La implementación actual usa el envío de formulario normal
                // Se mantiene para futura implementación AJAX
            });

            // Botones y acciones compartidas
            $(document).on('click', '.cliente-action', function () {
                const action = $(this).data('action');
                const clienteId = $(this).data('id');

                if (action && clienteId) {
                    // Procesar acciones específicas
                    if (action === 'delete' && !confirm('¿Está seguro que desea eliminar este cliente?')) {
                        e.preventDefault();
                    }
                }
            });
        },

        // CARGA DE CIUDADES

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
                    App.notify?.error('Error al cargar ciudades');
                }
            });
        },

        // FORMULARIO DE CLIENTE

        // Configurar manejo de Provincias y Ciudades
        setupProvinciasCiudades: function () {
            const self = this;

            $("#provincias").change(function () {
                const provinciaId = $(this).val();
                const ciudadId = $("#ciudades").val();
                self.loadCiudades(provinciaId, 'ciudades', ciudadId);
            });

            if ($("#provincias").val()) {
                $("#provincias").trigger("change");
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

            $('#cambiarGarante').on('click', function (e) {
                e.preventDefault();
                const clienteId = $('#ClienteID').val();
                window.location.href = `/Clientes/AsignarGarante/${clienteId}`;
            });
        },

        // ASIGNAR GARANTE

        setupAsignarGarante: function () {
            const self = this;

            // Cargar ciudades al cambiar provincia
            $('#ProvinciaID').change(function () {
                var provinciaID = $(this).val();
                self.loadCiudades(provinciaID, 'CiudadID');
            });

            // Trigger initial change if province is already selected
            if ($('#ProvinciaID').val()) {
                $('#ProvinciaID').trigger('change');
            }

            // Búsqueda de cliente potencial garante
            $('#buscarPotencialGarante').on('click', function () {
                const dni = $('#dniPotencialGarante').val();
                if (!dni) {
                    App.notify?.warning('Ingrese un DNI para buscar');
                    return;
                }

                $.ajax({
                    url: '/Clientes/BuscarPotencialGarante',
                    type: 'POST',
                    data: { dni: dni },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            $('#nombrePotencialGarante').val(response.data.nombre);
                            $('#limiteDisponible').val(response.data.limiteDisponible);
                            $('#GaranteID').val(response.data.id);

                            $('#seccionInfoGarante').removeClass('d-none');
                        } else {
                            App.notify?.warning(response.message || 'No se encontró un cliente con ese DNI');
                            $('#nombrePotencialGarante, #limiteDisponible, #GaranteID').val('');
                            $('#seccionInfoGarante').addClass('d-none');
                        }
                    },
                    error: function () {
                        App.notify?.error('Error al buscar potencial garante');
                    }
                });
            });

            // Búsqueda con Enter
            $('#dniPotencialGarante').keypress(function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#buscarPotencialGarante').click();
                    return false;
                }
            });
        },

        // VALIDACIÓN DE FORMULARIO

        validateClienteForm: function () {
            let isValid = true;

            // Validar campos requeridos
            $('form[id$="ClienteForm"] [required]').each(function () {
                if (!$(this).val()) {
                    isValid = false;
                    $(this).addClass('is-invalid');
                } else {
                    $(this).removeClass('is-invalid');
                }
            });

            // Validaciones específicas
            const dni = $('#DNI').val();
            if (dni && !/^\d{7,8}$/.test(dni)) {
                $('#DNI').addClass('is-invalid');
                $('<div class="invalid-feedback">El DNI debe tener entre 7 y 8 dígitos.</div>').insertAfter('#DNI');
                isValid = false;
            }

            const email = $('#Email').val();
            if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                $('#Email').addClass('is-invalid');
                $('<div class="invalid-feedback">Ingrese un email válido.</div>').insertAfter('#Email');
                isValid = false;
            }

            // Validar información de crédito si está habilitado
            if ($('#aptoCredito').is(':checked')) {
                const limiteCredito = parseFloat($('#LimiteCredito').val());
                if (isNaN(limiteCredito) || limiteCredito <= 0) {
                    $('#LimiteCredito').addClass('is-invalid');
                    $('<div class="invalid-feedback">El límite de crédito debe ser mayor a cero.</div>').insertAfter('#LimiteCredito');
                    isValid = false;
                }

                // Validar garante si es requerido
                if ($('#requiereGarante').is(':checked') && !$('#GaranteID').val()) {
                    $('#seccionGarante').addClass('border border-danger rounded p-2');
                    $('<div class="text-danger mt-1">Debe seleccionar un garante.</div>').appendTo('#seccionGarante');
                    isValid = false;
                }
            }

            return isValid;
        }
    };

    // Crear alias para compatibilidad con código existente
    App.clienteForm = {
        init: function () {
            App.clientesController.setupProvinciasCiudades();
            App.clientesController.setupCreditoFields();
        }
    };

    App.asignarGarante = {
        init: function () {
            App.clientesController.setupAsignarGarante();
        }
    };

})(window, jQuery);