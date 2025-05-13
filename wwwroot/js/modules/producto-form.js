// producto-form.js - Módulo para formularios de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.productoForm = {
        init: function () {
            this.setupRubroSubRubroHandlers();
            this.setupPriceCalculation();
            this.setupModalHandlers();
        },

        // Configurar manejadores de Rubro-SubRubro
        setupRubroSubRubroHandlers: function () {
            // Cambio de rubro
            $('#rubroDropdown').change(function () {
                var rubroId = $(this).val();
                App.productosController.loadSubRubros(rubroId, 'subRubroDropdown', $('#SelectedSubRubroID').val());

                // Actualizar también el select en el modal de SubRubro
                $('#rubroID').val(rubroId);
                $('#btnAddSubRubro').prop('disabled', !rubroId);
            });

            // Inicialización si ya hay rubro seleccionado
            if ($('#rubroDropdown').val()) {
                $('#rubroDropdown').trigger('change');
            } else {
                $('#btnAddSubRubro').prop('disabled', true);
            }
        },

        // Configurar cálculo de precios
        setupPriceCalculation: function () {
            // Obtener porcentajes de configuración
            const porcentajeGananciaPContado = parseFloat($('#porcentajeGananciaPContado').val()) || 50;
            const porcentajeGananciaPLista = parseFloat($('#porcentajeGananciaPLista').val()) || 84;

            // Actualizar precios al cambiar costo
            $('#pCosto').on('input', function () {
                const pCosto = parseFloat($(this).val()) || 0;
                const pContado = pCosto * (1 + porcentajeGananciaPContado / 100);
                const pLista = pCosto * (1 + porcentajeGananciaPLista / 100);

                $('#pContadoDisplay').val(pContado.toFixed(2));
                $('#pListaDisplay').val(pLista.toFixed(2));

                // Actualizar campos ocultos
                $('#PContado').val(pContado.toFixed(2));
                $('#PLista').val(pLista.toFixed(2));
            });

            // Llamar al inicio para establecer valores
            $('#pCosto').trigger('input');
        },

        // Configurar manejadores de modales
        setupModalHandlers: function () {
            // Modal de SubRubro
            $('#nuevoSubRubroModal').on('show.bs.modal', function (e) {
                var rubroId = $('#rubroDropdown').val();
                var rubroText = $('#rubroDropdown option:selected').text();

                if (!rubroId) {
                    $('#errorSubRubro').text('Debe seleccionar un Rubro primero');
                    $('#btnGuardarSubRubro').prop('disabled', true);
                    return;
                }

                $('#errorSubRubro').text('');
                $('#btnGuardarSubRubro').prop('disabled', false);
                $('#rubroID').empty().append($('<option></option>').val(rubroId).text(rubroText));
            });

            // Guardar nuevo Rubro
            $('#btnGuardarRubro').click(function () {
                var nombre = $('#nombreRubro').val();
                if (!nombre) {
                    $('#errorRubro').text('El nombre es obligatorio');
                    return;
                }

                App.ajax.post('/Catalogo/CreateRubroAjax', {
                    Nombre: nombre
                }, function (result) {
                    if (result.success) {
                        // Agregar nuevo rubro a la lista
                        $('#rubroDropdown').append($('<option></option>')
                            .val(result.id)
                            .text(result.name)
                            .attr('selected', 'selected')
                        );

                        // Limpiar modal y cerrarlo
                        $('#nombreRubro').val('');
                        $('#nuevoRubroModal').modal('hide');

                        // Actualizar SubRubros
                        $('#rubroDropdown').trigger('change');

                        // Mostrar notificación
                        App.notify.success('Rubro creado correctamente');
                    } else {
                        $('#errorRubro').text(result.message);
                    }
                }, function () {
                    $('#errorRubro').text('Error al crear el Rubro');
                });
            });

            // Guardar nuevo SubRubro
            $('#btnGuardarSubRubro').click(function () {
                var rubroId = $('#rubroID').val();
                var nombre = $('#nombreSubRubro').val();

                if (!rubroId) {
                    $('#errorSubRubro').text('Debe seleccionar un Rubro');
                    return;
                }

                if (!nombre) {
                    $('#errorSubRubro').text('El nombre es obligatorio');
                    return;
                }

                App.ajax.post('/Catalogo/CreateSubRubroAjax', {
                    Nombre: nombre,
                    RubroID: rubroId
                }, function (result) {
                    if (result.success) {
                        // Agregar nuevo subrubro a la lista
                        $('#subRubroDropdown').append($('<option></option>')
                            .val(result.id)
                            .text(result.name)
                            .attr('selected', 'selected')
                        );

                        // Limpiar modal y cerrarlo
                        $('#nombreSubRubro').val('');
                        $('#nuevoSubRubroModal').modal('hide');

                        // Mostrar notificación
                        App.notify.success('SubRubro creado correctamente');
                    } else {
                        $('#errorSubRubro').text(result.message);
                    }
                }, function () {
                    $('#errorSubRubro').text('Error al crear el SubRubro');
                });
            });

            // Guardar nueva Marca
            $('#btnGuardarMarca').click(function () {
                var nombre = $('#nombreMarca').val();
                if (!nombre) {
                    $('#errorMarca').text('El nombre es obligatorio');
                    return;
                }

                App.ajax.post('/Catalogo/CreateMarcaAjax', {
                    Nombre: nombre
                }, function (result) {
                    if (result.success) {
                        // Agregar nueva marca a la lista
                        $('#SelectedMarcaID').append($('<option></option>')
                            .val(result.id)
                            .text(result.name)
                            .attr('selected', 'selected')
                        );

                        // Limpiar modal y cerrarlo
                        $('#nombreMarca').val('');
                        $('#nuevaMarcaModal').modal('hide');

                        // Mostrar notificación
                        App.notify.success('Marca creada correctamente');
                    } else {
                        $('#errorMarca').text(result.message);
                    }
                }, function () {
                    $('#errorMarca').text('Error al crear la Marca');
                });
            });
        }
    };

})(window, jQuery);