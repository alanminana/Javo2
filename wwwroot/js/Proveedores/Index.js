    <script defer>
        (function($) {
            $(function() {
                var filters = {};
                function applyFilters() {
                    $.get('@Url.Action("Filter", "Proveedores")', filters)
                     .done(function(html) {
                         $('#proveedoresTableBody').html(html);
                     })
                     .fail(function() { alert('Error al aplicar filtros'); });
                }
                $('#applyFilter').on('click', function() {
                    filters = {
                        filterField: $('#filterField').val(),
                        filterValue: $('#filterValue').val()
                    };
                    applyFilters();
                });
                $('#filterValue').on('keypress', function(e) {
                    if (e.which === 13) { $('#applyFilter').click(); return false; }
                });
            });
        })(jQuery);
    </script>
