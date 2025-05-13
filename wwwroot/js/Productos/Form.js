        $(function() {
            const porcentajeGananciaPContado = 50; // Valor por defecto
            const porcentajeGananciaPLista = 84; // Valor por defecto

            // Función para actualizar los precios calculados
            function actualizarPrecios() {
                const pCosto = parseFloat($('#pCosto').val()) || 0;
                const pContado = pCosto * (1 + porcentajeGananciaPContado / 100);
                const pLista = pCosto * (1 + porcentajeGananciaPLista / 100);

                $('#pContadoDisplay').val(pContado.toFixed(2));
                $('#pListaDisplay').val(pLista.toFixed(2));

                // Actualizar los campos ocultos que se enviarán con el formulario
                $('#PContado').val(pContado.toFixed(2));
                $('#PLista').val(pLista.toFixed(2));
            }

            // Llamar a la función al cargar la página
            actualizarPrecios();

            // Actualizar precios cuando cambie el precio de costo
            $('#pCosto').on('input', function() {
                actualizarPrecios();
            });

            // Cargar subrubros cuando cambia el rubro
            $('#rubroDropdown').change(function() {
                var rubroId = $(this).val();
                cargarSubRubros(rubroId);
            });

            function cargarSubRubros(rubroId) {
                if (rubroId) {
                    // Actualizar lista desplegable de SubRubros
                    $.getJSON('@Url.Action("GetSubRubros")', { rubroId: rubroId }, function(data) {
                        var subRubroSelect = $('#subRubroDropdown');
                        var currentSubRubroId = '@Model.SelectedSubRubroID';

                        subRubroSelect.empty();
                        subRubroSelect.append($('<option></option>').val('').text('-- Seleccione SubRubro --'));

                        $.each(data, function(index, item) {
                            var option = $('<option></option>').val(item.value).text(item.text);
                            if (item.value == currentSubRubroId) {
                                option.attr('selected', 'selected');
                            }
                            subRubroSelect.append(option);
                        });
                    });

                    // Actualizar también el select en el modal de SubRubro
                    $('#rubroID').val(rubroId);
                    $('#btnAddSubRubro').prop('disabled', false);
                } else {
                    $('#subRubroDropdown').empty().append($('<option></option>').val('').text('-- Seleccione SubRubro --'));
                    $('#btnAddSubRubro').prop('disabled', true);
                }
            }

            // Si ya hay un rubro seleccionado al cargar la página, cargar sus subrubros
            var selectedRubroId = $('#rubroDropdown').val();
            if (selectedRubroId) {
                cargarSubRubros(selectedRubroId);
            } else {
                $('#btnAddSubRubro').prop('disabled', true);
            }

            // Al abrir el modal de SubRubro, cargar el rubro seleccionado
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
            $('#btnGuardarRubro').click(function() {
                var nombre = $('#nombreRubro').val();
                if (!nombre) {
                    $('#errorRubro').text('El nombre es obligatorio');
                    return;
                }

                $.ajax({
                    url: '@Url.Action("CreateRubroAjax", "Catalogo")',
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                        Nombre: nombre
                    },
                    success: function(result) {
                        if (result.success) {
                            // Agregar nuevo rubro a la lista desplegable
                            $('#rubroDropdown').append($('<option></option>')
                                .val(result.id)
                                .text(result.name)
                                .attr('selected', 'selected')
                            );

                            // Limpiar modal y cerrarlo
                            $('#nombreRubro').val('');
                            $('#nuevoRubroModal').modal('hide');

                            // Actualizar SubRubros
                            cargarSubRubros(result.id);

                            // Mostrar notificación
                            toastr.success('Rubro creado correctamente');
                        } else {
                            $('#errorRubro').text(result.message);
                        }
                    },
                    error: function() {
                        $('#errorRubro').text('Error al crear el Rubro');
                    }
                });
            });

            // Guardar nuevo SubRubro
            $('#btnGuardarSubRubro').click(function() {
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

                $.ajax({
                    url: '@Url.Action("CreateSubRubroAjax", "Catalogo")',
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                        Nombre: nombre,
                        RubroID: rubroId
                    },
                    success: function(result) {
                        if (result.success) {
                            // Agregar nuevo subrubro a la lista desplegable
                            $('#subRubroDropdown').append($('<option></option>')
                                .val(result.id)
                                .text(result.name)
                                .attr('selected', 'selected')
                            );

                            // Limpiar modal y cerrarlo
                            $('#nombreSubRubro').val('');
                            $('#nuevoSubRubroModal').modal('hide');

                            // Mostrar notificación
                            toastr.success('SubRubro creado correctamente');
                        } else {
                            $('#errorSubRubro').text(result.message);
                        }
                    },
                    error: function() {
                        $('#errorSubRubro').text('Error al crear el SubRubro');
                    }
                });
            });

            // Guardar nueva Marca
            $('#btnGuardarMarca').click(function() {
                var nombre = $('#nombreMarca').val();
                if (!nombre) {
                    $('#errorMarca').text('El nombre es obligatorio');
                    return;
                }

                $.ajax({
                    url: '@Url.Action("CreateMarcaAjax", "Catalogo")',
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                        Nombre: nombre
                    },
                    success: function(result) {
                        if (result.success) {
                            // Agregar nueva marca a la lista desplegable
                            $('#SelectedMarcaID').append($('<option></option>')
                                .val(result.id)
                                .text(result.name)
                                .attr('selected', 'selected')
                            );

                            // Limpiar modal y cerrarlo
                            $('#nombreMarca').val('');
                            $('#nuevaMarcaModal').modal('hide');

                            // Mostrar notificación
                            toastr.success('Marca creada correctamente');
                        } else {
                            $('#errorMarca').text(result.message);
                        }
                    },
                    error: function() {
                        $('#errorMarca').text('Error al crear la Marca');
                    }
                });
            });
        });
