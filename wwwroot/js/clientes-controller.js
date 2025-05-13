// clientes-controller.js - Controlador para operaciones de clientes
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.clientesController = {
        init: function () {
            this.setupEventHandlers();
        },

        setupEventHandlers: function () {
            // Filtrado de clientes
            $('#filtroCliente').on('submit', function (e) {
                // La implementación actual usa el envío de formulario normal
                // Se mantiene para futura implementación AJAX
            });
        },

        // Cargar ciudades al cambiar provincia
        loadCiudades: function (provinciaID, ciudadDropdownId, callback) {
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
                        ciudadSelect.append($('<option></option>').val(ciudad.value).text(ciudad.text));
                    });

                    if (callback && typeof callback === 'function') {
                        callback(ciudades);
                    }
                },
                error: function () {
                    App.notify.error('Error al cargar ciudades');
                }
            });
        }
    };

})(window, jQuery);