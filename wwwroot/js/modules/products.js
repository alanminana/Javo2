// products.js - Módulo unificado para gestión de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.products = {
        // Variables
        productoActual: {
            id: 0,
            codigoAlfa: '',
            codigoBarra: '',
            nombre: '',
            marca: '',
            precio: 0,
            precioLista: 0
        },

        // Inicialización
        init: function () {
            console.log("Módulo unificado de productos y catálogo inicializado");
            // Detectar contexto
            const isProductForm = $('#rubroDropdown').length > 0;
            const isProductList = $('#productosTableBody').length > 0;
            const isPriceAdjustment = $('#btnAplicarAjuste').length > 0;
            const isCatalogView = $('#rubrosTableBody, #marcasTableBody').length > 0;
            const isConfiguracion = $('#configuracionForm').length > 0;
            this.setupEventHandlers();

            // Inicializar submódulos
            if (this.precioAjuste) {
                this.precioAjuste.init();
            }
            if (isProductForm) {
                this.setupRubroSubRubroHandlers();
                this.setupModalHandlers();
                this.setupPriceCalculation();
            }

            if (isProductList || isPriceAdjustment) {
                this.initPriceCalculators();
            }

            if (isPriceAdjustment && this.precioAjuste) {
                this.precioAjuste.init();
            }

            if (isCatalogView) {
                this.initCatalogo();
            }

            if (isConfiguracion) {
                this.initConfiguracion();
            }
        },

        // BÚSQUEDA Y GESTIÓN BÁSICA DE PRODUCTOS

        searchByCode: function (url, code, options) {
            options = options || {};

            if (!code) {
                if (options.onError) options.onError("Ingrese un código para buscar");
                return;
            }

            App.ajax.post(url, { codigoProducto: code }, function (response) {
                if (response.success) {
                    const producto = {
                        id: response.data.productoID,
                        codigoAlfa: response.data.codigoAlfa || '',
                        codigoBarra: response.data.codigoBarra || '',
                        nombre: response.data.nombreProducto,
                        marca: response.data.marca || '',
                        precio: parseFloat(response.data.precioUnitario) || 0,
                        precioLista: parseFloat(response.data.precioLista) || 0
                    };

                    if (options.nameField) $(options.nameField).val(producto.nombre);
                    if (options.priceField) $(options.priceField).val(producto.precio.toFixed(2));
                    if (options.quantityField) {
                        $(options.quantityField).val(1);
                        $(options.quantityField).focus();
                    }

                    if (options.onSuccess) options.onSuccess(producto);
                } else {
                    if (options.modalErrorId) $(options.modalErrorId).modal('show');
                    else if (options.onError) options.onError("Producto no encontrado");
                    else App.notify.error("Producto no encontrado");

                    if (options.nameField) $(options.nameField).val('');
                    if (options.priceField) $(options.priceField).val('');

                    if (options.onNotFound) options.onNotFound();
                }
            }, function () {
                if (options.onError) options.onError("Error al buscar producto");
                else App.notify.error("Error al buscar producto");
            });
        },

        addToTable: function (tableId, producto, cantidad, options) {
            options = options || {};

            if (!producto || !producto.id) {
                App.notify.warning("Debe buscar un producto primero");
                return false;
            }

            const cantidadNum = parseInt(cantidad);
            if (isNaN(cantidadNum) || cantidadNum <= 0) {
                App.notify.warning("La cantidad debe ser mayor a cero");
                return false;
            }

            // Verificar si el producto ya está en la tabla
            let existe = false;
            let index = -1;

            $(`#${tableId} tbody tr`).each(function (i) {
                const productoID = $(this).find('input[name$=".ProductoID"]').val();
                if (parseInt(productoID) === producto.id) {
                    existe = true;
                    index = i;
                    return false; // Salir del bucle
                }
            });

            const subtotal = cantidadNum * producto.precio;

            if (existe) {
                // Actualizar cantidad
                const row = $(`#${tableId} tbody tr`).eq(index);
                const cantidadActual = parseInt(row.find('.cantidad').val());
                const nuevaCantidad = cantidadActual + cantidadNum;

                row.find('.cantidad').val(nuevaCantidad);
                row.find('.subtotal').text(App.format.currency(nuevaCantidad * producto.precio));
                row.find('input[name$=".PrecioTotal"]').val((nuevaCantidad * producto.precio).toFixed(2));
            } else {
                // Crear nueva fila
                const rowCount = $(`#${tableId} tbody tr`).length;
                const newRow = this.getProductRowTemplate(rowCount, producto, cantidadNum, subtotal, options.namePrefix || 'ProductosPresupuesto');
                $(`#${tableId} tbody`).append(newRow);
            }

            // Actualizar totales
            if (options.updateTotals && typeof options.updateTotals === 'function') {
                options.updateTotals();
            } else if (options.autoUpdateTotals !== false) {
                App.tables.updateTotals(tableId);
            }

            // Limpiar campos
            if (options.resetFields) {
                if (options.codeField) $(options.codeField).val('');
                if (options.nameField) $(options.nameField).val('');
                if (options.priceField) $(options.priceField).val('');
            }

            return true;
        },

        getProductRowTemplate: function (index, producto, cantidad, subtotal, namePrefix) {
            namePrefix = namePrefix || 'ProductosPresupuesto';

            return `
                <tr data-index="${index}">
                    <td>
                        <input type="hidden" name="${namePrefix}[${index}].ProductoID" value="${producto.id}" />
                        <input type="hidden" name="${namePrefix}[${index}].CodigoAlfa" value="${producto.codigoAlfa || ''}" />
                        <input type="hidden" name="${namePrefix}[${index}].CodigoBarra" value="${producto.codigoBarra || ''}" />
                        <input type="hidden" name="${namePrefix}[${index}].Marca" value="${producto.marca || ''}" />
                        <input type="hidden" name="${namePrefix}[${index}].NombreProducto" value="${producto.nombre}" />
                        <input type="hidden" name="${namePrefix}[${index}].PrecioUnitario" value="${producto.precio.toFixed(2)}" />
                        <input type="hidden" name="${namePrefix}[${index}].PrecioTotal" value="${subtotal.toFixed(2)}" />
                        <input type="hidden" name="${namePrefix}[${index}].PrecioLista" value="${(producto.precioLista || 0).toFixed(2)}" />
                        ${producto.codigoAlfa || producto.codigoBarra || producto.id}
                    </td>
                    <td>${producto.nombre}</td>
                    <td><input type="number" name="${namePrefix}[${index}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                    <td>${App.format.currency(producto.precio)}</td>
                    <td><span class="subtotal">${App.format.currency(subtotal)}</span></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        },

        // FORMULARIOS DE PRODUCTOS

        setupRubroSubRubroHandlers: function () {
            // Cambio de rubro
            $('#rubroDropdown').change(function () {
                var rubroId = $(this).val();

                if (!rubroId) {
                    $('#subRubroDropdown').empty().append('<option value="">-- Seleccione SubRubro --</option>');
                    return;
                }

                // Cargar subrubros
                App.products.loadSubRubros(rubroId, 'subRubroDropdown', $('#SelectedSubRubroID').val());

                // Actualizar select en modal
                $('#rubroID').val(rubroId);
                $('#btnAddSubRubro').prop('disabled', !rubroId);
            });

            // Inicialización si hay rubro seleccionado
            if ($('#rubroDropdown').val()) {
                $('#rubroDropdown').trigger('change');
            } else {
                $('#btnAddSubRubro').prop('disabled', true);
            }
        },

        loadSubRubros: function (rubroId, subRubroDropdownId, currentSubRubroId, url) {
            if (!rubroId) {
                $(`#${subRubroDropdownId}`).empty().append('<option value="">-- Seleccione SubRubro --</option>');
                return;
            }

            // Deshabilitar dropdown mientras carga
            $(`#${subRubroDropdownId}`).prop('disabled', true).empty().append('<option value="">Cargando...</option>');

            $.getJSON(url || '/Productos/GetSubRubros', { rubroId: rubroId }, function (data) {
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

                if (typeof App.notify !== 'undefined') {
                    App.notify.error("Error al cargar subrubros: " + errorThrown);
                }
            });
        },

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
        },

        // CÁLCULO DE PRECIOS DE PRODUCTOS

        setupPriceCalculation: function () {
            // Obtener porcentajes desde configuración
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

            // Llamar al inicio para establecer valores iniciales
            $('#pCosto').trigger('input');
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

        // CONTROLADOR Y EVENT HANDLERS

        setupEventHandlers: function () {
            this.setupEventListeners();
            this.setupActionHandlers();
            this.setupRubroSubRubroHandlers();
            this.setupModalHandlers();
            this.setupPriceCalculation();
        },

        // Configurar listeners de eventos
        setupEventListeners: function () {
            const self = this;

            // Búsqueda de productos
            $(document).on('click', '.buscar-producto-btn', function () {
                const codigoInput = $(this).closest('.input-group').find('input').first();
                const codigo = codigoInput.val();

                self.searchByCode('/Productos/BuscarProducto', codigo, {
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

                if (self.addToTable('productosTable', producto, cantidad, {
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

                App.ajax.post('/Productos/IncrementarPrecios', {
                    ProductoIDs: selectedProducts.join(','),
                    porcentaje: porcentaje,
                    isAumento: isAumento,
                    descripcion: descripcion
                }, function (result) {
                    if (result.success) {
                        App.notify.success(result.message);
                        // Recargar para mostrar los cambios
                        setTimeout(function () {
                            window.location.reload();
                        }, 1500);
                    } else {
                        App.notify.error(result.message);
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
        },

        // SUBMÓDULO DE AJUSTE DE PRECIOS

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
                    App.products.precioAjuste.actualizarDuracion();
                });

                // Seleccionar/deseleccionar todos
                $(document).on('change', '#checkAll', function () {
                    const isChecked = $(this).prop('checked');
                    $('.producto-check').prop('checked', isChecked);
                });

                // Botón de simulación
                $(document).on('click', '#btnSimular', function () {
                    App.products.precioAjuste.simularAjuste();
                });
            },

            // Configuración para ajustes definitivos
            setupAjusteDefinitivo: function () {
                // Filtrar productos por criterios
                $(document).on('change', '#filtroRubro, #filtroMarca, #filtroSubRubro', function () {
                    App.products.precioAjuste.filtrarProductos();
                });

                // Aplicar ajuste a seleccionados
                $(document).on('click', '#btnAplicarAjuste', function () {
                    App.products.precioAjuste.aplicarAjuste();
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
                App.ajax.post('/AjustePrecios/SimularAjusteTemporal', requestData, function (data) {
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
                        App.products.precioAjuste.renderizarTablaSimulacion(data.productos, esAumento);
                    } else {
                        App.notify.error('Error al simular: ' + data.message);
                    }
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
                    if (App.tables && App.tables.initSelectAll) {
                        App.tables.initSelectAll('#selectAllProducts', '.producto-check');
                    }
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
                            App.products.precioAjuste.filtrarProductos();
                        }, 1000);
                    } else {
                        App.notify.error('Error al aplicar el ajuste: ' + response.message);
                    }
                });
            }
        }
    },// CATÁLOGO Y PROMOCIONES

        // Inicialización del módulo de catálogo
        initCatalogo: function() {
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
                    App.notify?.error('Error al aplicar filtro');
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

    // MÓDULO DE CONFIGURACIÓN

    // Inicializar módulo de configuración
    initConfiguracion: function () {
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

    // GESTIÓN DE MARCAS

    // Crear nueva marca
    createMarca: function(nombre, callback) {
        if (!nombre || nombre.trim() === '') {
            App.notify?.warning('El nombre de la marca es obligatorio');
            return;
        }

        App.ajax.post('/Catalogo/CreateMarcaAjax', {
            Nombre: nombre
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('Marca creada correctamente');

                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al crear la marca');
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    },

    // Eliminar marca
    deleteMarca: function(id, callback) {
        if (!confirm('¿Está seguro que desea eliminar esta marca? Esta acción no se puede deshacer.')) {
            return;
        }
        App.ajax.post('/Catalogo/DeleteMarcaAjax', {
            id: id
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('Marca eliminada correctamente');
                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al eliminar la marca');
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    },

    // GESTIÓN DE RUBROS

    // Crear nuevo rubro
    createRubro: function(nombre, callback) {
        if (!nombre || nombre.trim() === '') {
            App.notify?.warning('El nombre del rubro es obligatorio');
            return;
        }
        App.ajax.post('/Catalogo/CreateRubroAjax', {
            Nombre: nombre
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('Rubro creado correctamente');
                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al crear el rubro');
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    },

    // Eliminar rubro
    deleteRubro: function(id, callback) {
        if (!confirm('¿Está seguro que desea eliminar este rubro? Esta acción afectará a todos los productos asociados.')) {
            return;
        }
        App.ajax.post('/Catalogo/DeleteRubroAjax', {  // <- URL CORREGIDA
            id: id
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('Rubro eliminado correctamente');  // <- MENSAJE CORREGIDO
                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al eliminar el rubro: ' + result.message);  // <- MENSAJE CORREGIDO
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    },

    // GESTIÓN DE SUBRUBROS

    // Crear nuevo subrubro
    createSubRubro: function(nombre, rubroID, callback) {
        if (!nombre || nombre.trim() === '') {
            App.notify?.warning('El nombre del subrubro es obligatorio');
            return;
        }
        if (!rubroID) {
            App.notify?.warning('Debe seleccionar un rubro');
            return;
        }
        App.ajax.post('/Catalogo/CreateSubRubroAjax', {
            Nombre: nombre,
            RubroID: rubroID
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('SubRubro creado correctamente');
                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al crear el subrubro');
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    },

    // Eliminar subrubro
    deleteSubRubro: function(id, callback) {
        if (!confirm('¿Está seguro que desea eliminar este subrubro? Esta acción afectará a todos los productos asociados.')) {
            return;
        }
        App.ajax.post('/Catalogo/DeleteSubRubroAjax', {
            id: id
        }, function (result) {
            if (result.success) {
                // Notificar éxito
                App.notify?.success('SubRubro eliminado correctamente');
                // Llamar callback con el resultado
                if (callback && typeof callback === 'function') {
                    callback(result);
                }
            } else {
                App.notify?.error(result.message || 'Error al eliminar el subrubro: ' + result.message);
            }
        }, function () {
            App.notify?.error('Error al comunicarse con el servidor');
        });
    }
}; // Cierre del objeto App.products

// Alias para compatibilidad con código existente
App.catalogo = {
    init: function () {
        App.products.initCatalogo();
    },
    setupFilterHandlers: App.products.setupFilterHandlers,
    setupSubRubrosEditor: App.products.setupSubRubrosEditor
};

App.configuracion = {
    init: function () {
        App.products.initConfiguracion();
    }
};

}) (window, jQuery); // Espacio añadido para mejor legibilidad