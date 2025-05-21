// proveedores-controller.js - Controlador para operaciones de proveedores
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.proveedoresController = {
        init: function () {
            this.setupFilterHandlers();
        },

        setupFilterHandlers: function () {
            $('#applyFilter').on('click', function () {
                const filters = {
                    filterField: $('#filterField').val(),
                    filterValue: $('#filterValue').val()
                };

                $.get('/Proveedores/Filter', filters)
                    .done(function (html) {
                        $('#proveedoresTableBody').html(html);
                    })
                    .fail(function () {
                        App.notify.error('Error al aplicar filtros');
                    });
            });

            $('#filterValue').on('keypress', function (e) {
                if (e.which === 13) {
                    $('#applyFilter').click();
                    return false;
                }
            });
        },

        // Búsqueda de productos para asignación
        searchProductsForAssignment: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify.warning('Ingrese al menos 2 caracteres para buscar');
                return;
            }

            $.ajax({
                url: '/Proveedores/SearchProductsForAssignment',
                method: 'POST',
                data: { term: term },
                success: function (data) {
                    if (callback && typeof callback === 'function') {
                        callback(data);
                    }
                },
                error: function () {
                    App.notify.error('Error al buscar productos');
                }
            });
        },

        // Búsqueda avanzada de productos para compra
        searchProductsForPurchase: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify.warning('Ingrese al menos 2 caracteres para buscar');
                return;
            }

            $.ajax({
                url: '/Proveedores/SearchProductsForPurchase',
                method: 'POST',
                data: { term: term },
                success: function (data) {
                    if (callback && typeof callback === 'function') {
                        callback(data);
                    }
                },
                error: function () {
                    App.notify.error('Error al buscar productos');
                }
            });
        }
    };

})(window, jQuery);