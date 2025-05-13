
        (function($) {
            $(function() {
                $('#applyFilter').on('click', function() {
                    var term = $('#filterValue').val();
                    $.get('@Url.Action("FilterAsync", "Catalogo")', { term: term })
                     .done(function(data) {
                        $('#rubrosTableBody').html(data.rubrosPartial);
                        $('#marcasTableBody').html(data.marcasPartial);
                     })
                     .fail(function() {
                        alert('Error al aplicar filtro');
                     });
                });
                $('#clearFilter').on('click', function() {
                    $('#filterValue').val('').trigger('input');
                    $('#applyFilter').click();
                });
            });
        })(jQuery);
