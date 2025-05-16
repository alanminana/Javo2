// cliente-form.js - Módulo para formularios de clientes
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.clienteForm = {
        init: function () {
            this.setupProvinciasCiudades();
            this.setupCreditoFields();
        },

        // Configurar manejo de Provincias y Ciudades
        setupProvinciasCiudades: function () {
            $("#provincias").change(function () {
                const provinciaId = $(this).val();
                const ciudadId = $("#ciudades").val();
                App.clientesController.loadCiudades(provinciaId, 'ciudades', ciudadId);
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
        }
    };

    // Módulo específico para asignar garante
    App.asignarGarante = {
        init: function () {
            // Cargar ciudades al cambiar provincia
            $('#ProvinciaID').change(function () {
                var provinciaID = $(this).val();
                App.clientesController.loadCiudades(provinciaID, 'CiudadID');
            });

            // Trigger initial change if province is already selected
            if ($('#ProvinciaID').val()) {
                $('#ProvinciaID').trigger('change');
            }
        }
    };

})(window, jQuery);