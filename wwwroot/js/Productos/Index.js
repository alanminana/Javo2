        $(document).ready(function() {
            // Configuración de toastr
            toastr.options = {
                "closeButton": true,
                "progressBar": true,
                "positionClass": "toast-top-right",
                "timeOut": "3000"
            };

            // Sincronizar checkboxes de selección
            $("#checkAll, #table-checkAll").on('change', function() {
                const isChecked = $(this).prop('checked');
                $('.producto-check').prop('checked', isChecked);
                $("#checkAll, #table-checkAll").prop('checked', isChecked);

                // Resaltar filas seleccionadas
                if (isChecked) {
                    $('.producto-check').closest('tr').addClass('table-primary');
                } else {
                    $('.producto-check').closest('tr').removeClass('table-primary');
                }
            });

            // Manejar el cambio de checkbox individual
            $(document).on('change', '.producto-check', function() {
                $(this).closest('tr').toggleClass('table-primary', this.checked);

                // Actualizar estado de "Seleccionar todos"
                const totalCheckboxes = $('.producto-check').length;
                const checkedCheckboxes = $('.producto-check:checked').length;
                $("#checkAll, #table-checkAll").prop('checked', totalCheckboxes === checkedCheckboxes);
            });

            // Filtrado
            $('#applyFilters').on('click', function() {
                const data = {};
                data[$('#filterType').val()] = $('#filterValue').val();

                $.ajax({
                    url: '@Url.Action("Filter", "Productos")',
                    type: 'GET',
                    data: data,
                    success: function(html) {
                        $('#productosTableBody').html(html);
                        $('.form-check-input').addClass('bg-secondary border-light');
                    },
                    error: function(xhr, status, error) {
                        toastr.error('Error al filtrar: ' + error);
                    }
                });
            });

            // Reset con Escape
            $(document).keyup(function(e) {
                if (e.key === 'Escape') {
                    $('#filterType').val('Nombre');
                    $('#filterValue').val('');
                    $('#applyFilters').trigger('click');
                }
            });

            // Ajuste de precios
            $('#btnAdjustPrices').on('click', function() {
                const selectedProducts = $('.producto-check:checked');

                if (selectedProducts.length === 0) {
                    toastr.warning('Seleccione al menos un producto para ajustar precios.');
                    return;
                }

                const ids = selectedProducts.map(function() {
                    return $(this).val();
                }).get();

                const porcentaje = parseFloat($('#porcentaje').val().replace(',', '.'));
                if (isNaN(porcentaje) || porcentaje <= 0 || porcentaje > 100) {
                    toastr.warning('Ingrese un porcentaje válido entre 0.01 y 100.');
                    return;
                }

                const esAumento = $('input[name="ajusteTipo"]:checked').val() === 'true';
                const descripcion = $('#descripcionAjuste').val() || 'Ajuste desde listado de productos';

                Swal.fire({
                    title: 'Confirmar Ajuste de Precios',
                    html: `¿Está seguro de aplicar un <strong>${esAumento ? 'aumento' : 'descuento'} del ${porcentaje}%</strong> a <strong>${ids.length}</strong> productos?`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, aplicar ajuste',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33'
                }).then((result) => {
                    if (result.isConfirmed) {
                        Swal.fire({
                            title: 'Procesando...',
                            text: 'Aplicando ajuste de precios',
                            allowOutsideClick: false,
                            didOpen: () => {
                                Swal.showLoading();
                            }
                        });

                        // Enviar solicitud AJAX
                        $.ajax({
                            url: '@Url.Action("IncrementarPrecios", "Productos")',
                            type: 'POST',
                            data: {
                                ProductoIDs: ids.join(','),
                                porcentaje: porcentaje,
                                isAumento: esAumento,
                                descripcion: descripcion
                            },
                            headers: {
                                'RequestVerificationToken': $('#__RequestVerificationToken').val()
                            },
                            success: function(response) {
                                if (response.success) {
                                    Swal.fire({
                                        title: '¡Éxito!',
                                        text: response.message,
                                        icon: 'success',
                                        confirmButtonText: 'Ver detalles',
                                        showCancelButton: true,
                                        cancelButtonText: 'Cerrar'
                                    }).then((result) => {
                                        if (result.isConfirmed && response.ajusteId) {
                                            window.location.href = '@Url.Action("Details", "AjustePrecios")/' + response.ajusteId;
                                        } else {
                                            location.reload();
                                        }
                                    });
                                } else {
                                    Swal.fire('Error', response.message || 'Error al realizar el ajuste', 'error');
                                }
                            },
                            error: function(xhr, status, error) {
                                Swal.fire('Error', 'Ocurrió un error al comunicarse con el servidor', 'error');
                            }
                        });
                    }
                });
            });

            // Auto-expandir el panel de ajuste de precios si hay productos seleccionados
            $('.producto-check').on('change', function() {
                if ($('.producto-check:checked').length > 0 && !$('#ajustePreciosPanel').hasClass('show')) {
                    $('#ajustePreciosPanel').collapse('show');
                }
            });
        });
