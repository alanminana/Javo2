// catalogo-producto.js - Módulo unificado para catálogo y productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.catalogoProducto = {
        init: function () {
            this.setupFilterHandlers();
            this.setupSubRubrosEditor();
            this.setupRubroSubRubroHandlers();
            this.setupPriceCalculation();
            this.setupModalHandlers();

            // Inicializar submódulos si están en la página
            if (document.getElementById('configuracionForm')) {
                this.setupConfiguracion();
            }
        },

        // Configurar filtros
        setupFilterHandlers: function () {
            $('#applyFilter').on('click', function () {
                var term = $('#filterValue').val();
                $.get('/Catalogo/FilterAsync', { term: term })
                    .done(function (data) {
                        $('#rubrosTableBody').html(data.rubrosPartial);
                        $('#marcasTableBody').html(data.marcasPartial);
                    })
                    .fail(function () {
                        App.notify.error('Error al aplicar filtro');
                    });
            });

            $('#clearFilter').on('click', function () {
                $('#filterValue').val('').trigger('input');
                $('#applyFilter').click();
            });
        },

        // Configurar editor de SubRubros
        setupSubRubrosEditor: function () {
            var counter = $('#subRubrosTable tbody tr').length;

            $('#addSubRubro').on('click', function () {
                var name = $('#newSubRubro').val().trim();
                if (!name) return;

                var idx = counter++;
                var row = `<tr>
                   <td>
                       <input type="hidden" name="SubRubros[${idx}].ID" value="0" />
                       <input type="text" name="SubRubros[${idx}].Nombre" class="form-control bg-secondary text-light" value="${name}" />
                   </td>
                   <td class="text-center align-middle">
                       <button type="button" class="btn btn-sm btn-outline-danger delete-subrubro" title="Eliminar este subrubro">
                           <i class="bi bi-trash" aria-hidden="true"></i>
                           <span class="visually-hidden">Eliminar</span>
                       </button>
                       <input type="hidden" name="SubRubros[${idx}].IsDeleted" value="false" />
                   </td>
               </tr>`;

                $('#subRubrosTable tbody').append(row);
                $('#newSubRubro').val('');
            });

            $(document).on('click', '.delete-subrubro', function () {
                var tr = $(this).closest('tr');
                tr.find('input[name$=".IsDeleted"]').val('true');
                tr.hide();
            });
        },

        // Configurar selección de Rubros y SubRubros
        setupRubroSubRubroHandlers: function () {
            // Cambio de rubro
            $('#rubroDropdown').change(function () {
                var rubroId = $(this).val();

                if (!rubroId) {
                    $('#subRubroDropdown').empty().append('<option value="">-- Seleccione SubRubro --</option>');
                    return;
                }

                // Hacer la llamada AJAX directamente
                var currentSubRubroId = $('#SelectedSubRubroID').val();
                $('#subRubroDropdown').prop('disabled', true).empty().append('<option value="">Cargando...</option>');

                $.getJSON('/Productos/GetSubRubros', { rubroId: rubroId }, function (data) {
                    var dropdown = $('#subRubroDropdown');
                    dropdown.empty().append('<option value="">-- Seleccione SubRubro --</option>').prop('disabled', false);

                    $.each(data, function (i, item) {
                        var option = $('<option></option>').val(item.value).text(item.text);
                        if (currentSubRubroId && item.value == currentSubRubroId) {
                            option.attr('selected', 'selected');
                        }
                        dropdown.append(option);
                    });
                }).fail(function () {
                    $('#subRubroDropdown').prop('disabled', false)
                        .empty().append('<option value="">Error al cargar subrubros</option>');
                });

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

        // Configurar edición de configuración
        setupConfiguracion: function () {
            const tipoDato = $('#configuracionForm').data('tipoDato') || '';
            const valorInput = $('#Valor');

            valorInput.on('input', function () {
                let isValid = true;
                const valor = $(this).val();

                if (tipoDato === 'int') {
                    isValid = /^\d+$/.test(valor);
                } else if (tipoDato === 'decimal') {
                    isValid = /^\d+(\.\d+)?$/.test(valor);
                }

                if (isValid) {
                    $(this).removeClass('is-invalid').addClass('is-valid');
                } else {
                    $(this).removeClass('is-valid').addClass('is-invalid');
                }
            });
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

    // Compatibilidad con código existente
    App.catalogo = App.catalogoProducto;
    App.productoForm = App.catalogoProducto;
    App.configuracion = {
        init: function () {
            App.catalogoProducto.setupConfiguracion();
        }
    };

})(window, jQuery);