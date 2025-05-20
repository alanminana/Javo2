// productos-controller.js - Controlador centralizado para operaciones de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.productosController = {
        init: function () {
            this.setupActionHandlers();
            this.initPriceCalculators();

            // Inicializar submódulo de ajuste de precios si existe
            if (this.precioAjuste) {
                this.precioAjuste.init();
            }
        },

        // Configurar listeners de eventos
        setupEventListeners: function () {
            // Búsqueda de productos
            $(document).on('click', '.buscar-producto-btn', function () {
                const codigoInput = $(this).closest('.input-group').find('input').first();
                const codigo = codigoInput.val();

                App.products.searchByCode('/Productos/BuscarProducto', codigo, {
                    nameField: '#productoNombre',
                    priceField: '#productoPrecio',
                    quantityField: '#productoCantidad',
                    modalErrorId: '#productoNoEncontradoModal',
                    onSuccess: function (producto) {
                        // Almacenar producto actual en data del formulario
                        const form = codigoInput.closest('form');
                        form.data('productoActual', producto);
                    }
                });
            });

            // Agregar producto a tabla
            $(document).on('click', '.agregar-producto-btn', function () {
                const form = $(this).closest('form');
                const producto = form.data('productoActual');
                const cantidad = $('#productoCantidad').val();

                if (App.products.addToTable('productosTable', producto, cantidad, {
                    resetFields: true,
                    codeField: '#productoCodigo',
                    nameField: '#productoNombre',
                    priceField: '#productoPrecio'
                })) {
                    // Limpiar producto actual
                    form.data('productoActual', null);
                }
            });

            // Buscar al presionar Enter
            $(document).on('keypress', '.producto-codigo-input', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $(this).closest('.input-group').find('.buscar-producto-btn').click();
                }
            });
        },

        // Inicializar calculadoras de precios
        initPriceCalculators: function () {
            $(document).on('input', '#pCosto', function () {
                const pCosto = parseFloat($(this).val()) || 0;
                const pContadoPorc = parseFloat($('#porcentajeGananciaPContado').val()) || 50;
                const pListaPorc = parseFloat($('#porcentajeGananciaPLista').val()) || 84;

                const pContado = pCosto * (1 + pContadoPorc / 100);
                const pLista = pCosto * (1 + pListaPorc / 100);

                // Actualizar campos visibles
                $('#pContadoDisplay').val(pContado.toFixed(2));
                $('#pListaDisplay').val(pLista.toFixed(2));

                // Actualizar campos ocultos
                $('#PContado').val(pContado.toFixed(2));
                $('#PLista').val(pLista.toFixed(2));
            });
        },

        loadSubRubros: function (rubroId, subRubroDropdownId, currentSubRubroId, url) {
            if (!rubroId) {
                $(`#${subRubroDropdownId}`).empty().append('<option value="">-- Seleccione SubRubro --</option>');
                return;
            }

            console.log("Cargando subrubros para rubroId:", rubroId);

            // Deshabilitar dropdown mientras carga
            $(`#${subRubroDropdownId}`).prop('disabled', true).empty().append('<option value="">Cargando...</option>');

            $.getJSON('/Productos/GetSubRubros', { rubroId: rubroId }, function (data) {
                console.log("SubRubros recibidos:", data);

                const subRubroSelect = $(`#${subRubroDropdownId}`);
                subRubroSelect.empty().append('<option value="">-- Seleccione SubRubro --</option>').prop('disabled', false);

                $.each(data, function (index, item) {
                    const option = $('<option></option>').val(item.value).text(item.text);
                    if (currentSubRubroId && item.value == currentSubRubroId) {
                        option.attr('selected', 'selected');
                    }
                    subRubroSelect.append(option);
                });
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.error("Error al cargar subrubros:", textStatus, errorThrown);
                $(`#${subRubroDropdownId}`).prop('disabled', false).empty()
                    .append('<option value="">-- Error al cargar subrubros --</option>');

                // Mostrar notificación de error si disponible
                if (typeof App.notify !== 'undefined') {
                    App.notify.error("Error al cargar subrubros: " + errorThrown);
                }
            });
        },  // COMA AÑADIDA AQUÍ

        // Ajuste de precios por porcentaje
        calculateAdjustedPrices: function (productos, porcentaje, esAumento) {
            const factor = esAumento ? (1 + porcentaje / 100) : (1 - porcentaje / 100);

            return productos.map(function (p) {
                const costoNuevo = p.costo * factor;
                const contadoNuevo = p.contado * factor;
                const listaNuevo = p.lista * factor;

                return {
                    ...p,
                    costoNuevo: costoNuevo,
                    contadoNuevo: contadoNuevo,
                    listaNuevo: listaNuevo
                };
            });
        },

        // Submódulo para ajuste de precios (anteriormente precio-ajuste.js)
        precioAjuste: {
            init: function () {
                this.setupEventListeners();
            },

            setupEventListeners: function () {
                // Listeners para ajustes temporales
                this.setupAjusteTemporal();

                // Listeners para ajustes definitivos
                this.setupAjusteDefinitivo();

                // Confirmación de revertir
                $(document).on('click', '.btn-revertir-ajuste', function (e) {
                    if (!confirm('¿Está seguro que desea revertir este ajuste? Los precios volverán a sus valores anteriores.')) {
                        e.preventDefault();
                        return false;
                    }
                    return true;
                });
            },

            // Configuración para ajustes temporales (promociones)
            setupAjusteTemporal: function () {
                // Actualizar duración al cambiar fechas
                $(document).on('change', '#FechaInicio, #FechaFin', function () {
                    App.productosController.precioAjuste.actualizarDuracion();
                });

                // Seleccionar/deseleccionar todos
                $(document).on('change', '#checkAll', function () {
                    const isChecked = $(this).prop('checked');
                    $('.producto-check').prop('checked', isChecked);
                });

                // Botón de simulación
                $(document).on('click', '#btnSimular', function () {
                    App.productosController.precioAjuste.simularAjuste();
                });
            },

            // Configuración para ajustes definitivos
            setupAjusteDefinitivo: function () {
                // Filtrar productos por criterios
                $(document).on('change', '#filtroRubro, #filtroMarca, #filtroSubRubro', function () {
                    App.productosController.precioAjuste.filtrarProductos();
                });

                // Aplicar ajuste a seleccionados
                $(document).on('click', '#btnAplicarAjuste', function () {
                    App.productosController.precioAjuste.aplicarAjuste();
                });
            },

            // Calcular duración en días entre fechas
            actualizarDuracion: function () {
                const fechaInicio = new Date(document.getElementById('FechaInicio').value);
                const fechaFin = new Date(document.getElementById('FechaFin').value);

                if (!isNaN(fechaInicio) && !isNaN(fechaFin)) {
                    const diff = Math.ceil((fechaFin - fechaInicio) / (1000 * 60 * 60 * 24));
                    document.getElementById('duracion').value = diff > 0 ? diff : 0;
                } else {
                    document.getElementById('duracion').value = '';
                }
            },

            // Simular ajuste de precios
            simularAjuste: function () {
                // Obtener productos seleccionados
                const productosSeleccionados = Array.from(
                    document.querySelectorAll('.producto-check:checked')
                ).map(cb => parseInt(cb.dataset.id));

                if (!productosSeleccionados.length) {
                    App.notify.warning('Debe seleccionar al menos un producto para simular.');
                    return;
                }

                // Verificar porcentaje
                const porcentaje = parseFloat(document.getElementById('Porcentaje').value);
                if (isNaN(porcentaje) || porcentaje <= 0 || porcentaje > 100) {
                    App.notify.warning('Ingrese un porcentaje válido entre 0.01 y 100.');
                    return;
                }

                // Verificar fechas
                const fechaInicio = new Date(document.getElementById('FechaInicio').value);
                const fechaFin = new Date(document.getElementById('FechaFin').value);
                if (isNaN(fechaInicio) || isNaN(fechaFin)) {
                    App.notify.warning('Seleccione fechas válidas.');
                    return;
                }
                if (fechaInicio >= fechaFin) {
                    App.notify.warning('La fecha de inicio debe ser anterior a la fecha de finalización.');
                    return;
                }

                // Verificar tipo de ajuste
                const tipoAjuste = document.getElementById('TipoAjuste').value;
                if (!tipoAjuste) {
                    App.notify.warning('Seleccione un motivo para el ajuste.');
                    return;
                }

                // Preparar datos para la simulación
                const esAumento = document.getElementById('radioAumento').checked;
                const requestData = {
                    ProductoIDs: productosSeleccionados,
                    Porcentaje: porcentaje,
                    EsAumento: esAumento,
                    FechaInicio: fechaInicio,
                    FechaFin: fechaFin,
                    TipoAjuste: tipoAjuste
                };

                // Enviar solicitud AJAX
                fetch('/AjustePrecios/SimularAjusteTemporal', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(requestData)
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            // Mostrar contenedor de simulación
                            document.getElementById('simulacionContainer').classList.remove('d-none');

                            // Actualizar resumen
                            document.getElementById('simTipoAjuste').textContent = data.tipoAjuste;
                            document.getElementById('simOperacion').textContent = esAumento ? 'AUMENTO' : 'DESCUENTO';
                            document.getElementById('simOperacion').className = esAumento ? 'badge bg-success' : 'badge bg-danger';
                            document.getElementById('simPorcentaje').textContent = porcentaje + '%';
                            document.getElementById('simFechaInicio').textContent = data.fechaInicio;
                            document.getElementById('simFechaFin').textContent = data.fechaFin;
                            document.getElementById('simDuracion').textContent = data.duracion;

                            // Construir tabla de simulación
                            this.renderizarTablaSimulacion(data.productos, esAumento);
                        } else {
                            App.notify.error('Error al simular: ' + data.message);
                        }
                    })
                    .catch(error => {
                        console.error('Error en la solicitud:', error);
                        App.notify.error('Error al procesar la simulación. Consulte la consola para más detalles.');
                    });
            },

            // Renderizar tabla de simulación de ajuste
            renderizarTablaSimulacion: function (productos, esAumento) {
                const tbody = document.getElementById('simulacionBody');
                tbody.innerHTML = '';

                productos.forEach(p => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${p.nombre}</td>
                        <td class="text-end">${p.costoActual.toFixed(2)}</td>
                        <td class="text-end">${p.contadoActual.toFixed(2)}</td>
                        <td class="text-end">${p.listaActual.toFixed(2)}</td>
                        <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.costoNuevo.toFixed(2)}</td>
                        <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.contadoNuevo.toFixed(2)}</td>
                        <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.listaNuevo.toFixed(2)}</td>
                    `;
                    tbody.appendChild(row);
                });

                // Scroll hacia la simulación
                document.getElementById('simulacionContainer').scrollIntoView({ behavior: 'smooth', block: 'start' });
            },

            // Filtrar productos según criterios seleccionados
            filtrarProductos: function () {
                const rubroId = $('#filtroRubro').val();
                const marcaId = $('#filtroMarca').val();
                const subRubroId = $('#filtroSubRubro').val();

                // Preparar datos del filtro
                const requestData = {
                    RubroID: rubroId || 0,
                    MarcaID: marcaId || 0,
                    SubRubroID: subRubroId || 0
                };

                // Mostrar indicador de carga
                $('#productosContainer').html('<div class="text-center p-5"><div class="spinner-border"></div><p class="mt-2">Cargando productos...</p></div>');

                // Enviar solicitud AJAX
                App.ajax.post('/AjustePrecios/FiltrarProductos', requestData, function (response) {
                    $('#productosContainer').html(response);

                    // Re-inicializar componentes
                    App.tables.initSelectAll('#selectAllProducts', '.producto-check');
                });
            },

            // Aplicar ajuste de precios
            aplicarAjuste: function () {
                // Obtener productos seleccionados
                const productosSeleccionados = Array.from(
                    document.querySelectorAll('.producto-check:checked')
                ).map(cb => parseInt(cb.dataset.id));

                if (!productosSeleccionados.length) {
                    App.notify.warning('Debe seleccionar al menos un producto.');
                    return;
                }

                // Verificar porcentaje
                const porcentaje = parseFloat(document.getElementById('Porcentaje').value);
                if (isNaN(porcentaje) || porcentaje <= 0 || porcentaje > 100) {
                    App.notify.warning('Ingrese un porcentaje válido entre 0.01 y 100.');
                    return;
                }

                // Verificar tipo de ajuste
                const tipoAjuste = document.getElementById('TipoAjuste').value;
                if (!tipoAjuste) {
                    App.notify.warning('Seleccione un motivo para el ajuste.');
                    return;
                }

                // Confirmar operación
                const esAumento = document.getElementById('radioAumento').checked;
                const operacion = esAumento ? 'AUMENTAR' : 'REDUCIR';

                if (!confirm(`¿Está seguro que desea ${operacion} los precios de los productos seleccionados en un ${porcentaje}%? Esta acción no se puede deshacer directamente.`)) {
                    return;
                }

                // Preparar datos
                const requestData = {
                    ProductoIDs: productosSeleccionados,
                    Porcentaje: porcentaje,
                    EsAumento: esAumento,
                    TipoAjuste: tipoAjuste
                };

                // Enviar solicitud
                App.ajax.post('/AjustePrecios/AplicarAjuste', requestData, function (response) {
                    if (response.success) {
                        App.notify.success('Ajuste aplicado correctamente.');

                        // Recargar la lista de productos
                        setTimeout(function () {
                            App.productosController.precioAjuste.filtrarProductos();
                        }, 1000);
                    } else {
                        App.notify.error('Error al aplicar el ajuste: ' + response.message);
                    }
                });
            }
        },

        // Configurar manejadores de acciones
        setupActionHandlers: function () {
            // Manejo de ajuste de precios en la vista Index
            $(document).on('click', '#btnAdjustPrices', function () {
                const selectedProducts = $('.producto-check:checked').map(function () {
                    return $(this).val();
                }).get();

                if (selectedProducts.length === 0) {
                    App.notify.warning('Seleccione al menos un producto para ajustar precios.');
                    return;
                }

                const porcentaje = parseFloat($('#porcentaje').val());
                if (isNaN(porcentaje) || porcentaje <= 0) {
                    App.notify.warning('Ingrese un porcentaje válido mayor a cero.');
                    return;
                }

                const isAumento = $('#radioAumento').prop('checked');
                const descripcion = $('#descripcionAjuste').val();

                if (!confirm(`¿Está seguro que desea ${isAumento ? 'aumentar' : 'reducir'} en un ${porcentaje}% los precios de ${selectedProducts.length} productos?`)) {
                    return;
                }

                $.ajax({
                    url: '/Productos/IncrementarPrecios',
                    type: 'POST',
                    data: {
                        ProductoIDs: selectedProducts.join(','),
                        porcentaje: porcentaje,
                        isAumento: isAumento,
                        descripcion: descripcion
                    },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (result) {
                        if (result.success) {
                            App.notify.success(result.message);
                            // Recargar para mostrar los cambios
                            setTimeout(function () {
                                window.location.reload();
                            }, 1500);
                        } else {
                            App.notify.error(result.message);
                        }
                    },
                    error: function () {
                        App.notify.error('Error al procesar la solicitud. Intente nuevamente.');
                    }
                });
            });

            // Seleccionar/deseleccionar todos los productos
            $('#table-checkAll').on('change', function () {
                $('.producto-check').prop('checked', $(this).prop('checked'));
            });

            // Filtrar productos
            $('#applyFilters').on('click', function () {
                const filterType = $('#filterType').val();
                const filterValue = $('#filterValue').val();

                // Crear objeto para enviar
                const filters = {
                    Nombre: filterType === 'Nombre' ? filterValue : null,
                    Codigo: filterType === 'Codigo' ? filterValue : null,
                    Rubro: filterType === 'Rubro' ? filterValue : null,
                    SubRubro: filterType === 'SubRubro' ? filterValue : null,
                    Marca: filterType === 'Marca' ? filterValue : null
                };

                $.ajax({
                    url: '/Productos/Filter',
                    type: 'GET',
                    data: filters,
                    success: function (data) {
                        $('#productosTableBody').html(data);
                    },
                    error: function () {
                        App.notify.error('Error al filtrar productos');
                    }
                });
            });
        }
    };

})(window, jQuery);