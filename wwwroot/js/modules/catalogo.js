// catalogo.js - Módulo para catálogo y promociones
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.catalogo = {
        init: function () {
            this.setupFilterHandlers();
            this.setupSubRubrosEditor();
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
        }
    };

    // Módulo para edición de configuración
    App.configuracion = {
        init: function () {
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
        }
    };

})(window, jQuery);