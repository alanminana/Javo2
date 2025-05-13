        (function($) {
            $(function() {
                var counter = @Model.SubRubros.Count;
                $('#addSubRubro').on('click', function() {
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
                $(document).on('click', '.delete-subrubro', function() {
                    var tr = $(this).closest('tr');
                    tr.find('input[name$=".IsDeleted"]').val('true');
                    tr.hide();
                });
            });
        })(jQuery);
