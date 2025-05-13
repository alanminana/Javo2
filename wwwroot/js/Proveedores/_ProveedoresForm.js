
    <script defer>
        (function($) {
            $(function() {
                var assignedProducts = [];

                // Inicializar la lista de productos asignados
                $('#assignedProducts tr').each(function() {
                    assignedProducts.push(parseInt($(this).data('id')));
                });

                // Buscar productos
                $('#searchProductBtn').on('click', function() {
                    var searchTerm = $('#productSearch').val();
                    if (!searchTerm) return;

                    $.ajax({
                        url: '@Url.Action("SearchProductsForAssignment", "Proveedores")',
                        method: 'POST',
                        data: { term: searchTerm },
                        success: function(data) {
                            // Limpiar resultados anteriores
                            $('#productResultsTable tbody').empty();

                            if (data.length === 0) {
                                $('#productResultsTable tbody').append(
                                    '<tr><td colspan="3" class="text-center">No se encontraron productos</td></tr>'
                                );
                            } else {
                                // Mostrar productos
                                data.forEach(function(product) {
                                    var isAssigned = assignedProducts.includes(product.id);
                                    var assignBtn = isAssigned ?
                                        '<button type="button" class="btn btn-sm btn-secondary" disabled>Asignado</button>' :
                                        '<button type="button" class="btn btn-sm btn-primary assign-product" data-id="' + product.id +
                                        '" data-name="' + product.name + '" data-marca="' + product.marca + '">Asignar</button>';

                                    $('#productResultsTable tbody').append(
                                        '<tr>' +
                                        '<td>' + product.name + '</td>' +
                                        '<td>' + product.marca + '</td>' +
                                        '<td>' + assignBtn + '</td>' +
                                        '</tr>'
                                    );
                                });
                            }

                            // Mostrar resultados
                            $('#searchResults').show();
                        },
                        error: function() {
                            alert('Error al buscar productos');
                        }
                    });
                });

                // Asignar producto al hacer clic en el botón Asignar
                $(document).on('click', '.assign-product', function() {
                    var id = $(this).data('id');
                    var name = $(this).data('name');
                    var marca = $(this).data('marca');

                    if (!assignedProducts.includes(id)) {
                        assignedProducts.push(id);

                        // Agregar producto a la tabla de asignados
                        $('#assignedProducts').append(
                            '<tr data-id="' + id + '">' +
                            '<td>' + name + '</td>' +
                            '<td>' + marca + '</td>' +
                            '<td>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger remove-product">' +
                            '<i class="bi bi-trash"></i>' +
                            '</button>' +
                            '<input type="hidden" name="ProductosAsignados" value="' + id + '" />' +
                            '</td>' +
                            '</tr>'
                        );

                        // Actualizar botón en resultados
                        $(this).replaceWith('<button type="button" class="btn btn-sm btn-secondary" disabled>Asignado</button>');
                    }
                });

                // Eliminar producto
                $(document).on('click', '.remove-product', function() {
                    var row = $(this).closest('tr');
                    var id = row.data('id');

                    // Eliminar de la lista
                    assignedProducts = assignedProducts.filter(function(pid) {
                        return pid !== id;
                    });

                    // Eliminar fila
                    row.remove();

                    // Actualizar botón en resultados si está visible
                    $('#productResultsTable .assign-product[data-id="' + id + '"]').replaceWith(
                        '<button type="button" class="btn btn-sm btn-primary assign-product" data-id="' + id +
                        '" data-name="' + $(this).data('name') + '" data-marca="' + $(this).data('marca') + '">Asignar</button>'
                    );
                });

                // Al presionar Enter en la búsqueda
                $('#productSearch').keypress(function(e) {
                    if (e.which === 13) {
                        $('#searchProductBtn').click();
                        return false;
                    }
                });
            });
        })(jQuery);
    </script>
