// producto.controller.js - Controlador unificado para productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.productoController = {
        // Producto actual (para búsquedas)
        productoActual: null,

        init: function () {
            this.setupEventHandlers();
            this.initPriceCalculators();
            this.setupFilterHandlers();

            // Inicializar submódulos si existen
            if (this.precioAjuste) {
                this.precioAjuste.init();
            }
        },

        // Configurar manejadores de eventos generales
        setupEventHandlers: function () {
            const self = this;

            // Búsqueda de productos
            $(document).on('click', '.buscar-producto-btn', function () {
                const codigoInput = $(this).closest('.input-group').find('input').first();
                const codigo = codigoInput.val();
                const options = $(this).data('options') || {};

                self.searchProductByCode(codigo, options);
            });

            // Agregar producto a tabla
            $(document).on('click', '.agregar-producto-btn', function () {
                const form = $(this).closest('form');
                const producto = form.data('productoActual');
                const cantidad = $('#productoCantidad').val();
                const tableId = $(this).data('table') || 'productosTable';

                if (self.addProductToTable(tableId, producto, cantidad)) {
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

            // Manejo de ajuste de precios
            this.setupAjustePreciosEvents();
        },

        // Buscar producto por código
        searchProductByCode: function (codigo, options) {
            const self = this;
            options = options || {};

            const searchUrl = options.url || '/Productos/BuscarProducto';
            const nameField = options.nameField || '#productoNombre';
            const priceField = options.priceField || '#productoPrecio';
            const quantityField = options.quantityField || '#productoCantidad';
            const errorModal = options.errorModal || '#productoNoEncontradoModal';

            if (!codigo) {
                App.notify.warning('Ingrese un código para buscar');
                return;
            }

            $.ajax({
                url: searchUrl,
                type: 'POST',
                data: { codigoProducto: codigo },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        // Crear objeto producto con datos normalizados
                        self.productoActual = {
                            id: response.data.productoID,
                            codigoAlfa: response.data.codigoAlfa || '',
                            codigoBarra: response.data.codigoBarra || '',
                            nombre: response.data.nombreProducto,
                            marca: response.data.marca || '',
                            precio: parseFloat(response.data.precioUnitario) || 0,
                            precioLista: parseFloat(response.data.precioLista) || 0,
                            costo: parseFloat(response.data.precioCosto) || 0
                        };

                        // Mostrar datos en campos
                        $(nameField).val(self.productoActual.nombre);
                        $(priceField).val(self.productoActual.precio.toFixed(2));

                        if (quantityField) {
                            $(quantityField).val(1).focus();
                        }

                        // Si hay callback de éxito
                        if (options.onSuccess && typeof options.onSuccess === 'function') {
                            options.onSuccess(self.productoActual);
                        }

                        // Almacenar en el formulario para acceso fácil
                        const form = $(nameField).closest('form');
                        if (form.length) {
                            form.data('productoActual', self.productoActual);
                        }
                    } else {
                        // Mostrar error
                        $(errorModal).modal('show');
                        self.productoActual = null;

                        // Limpiar campos
                        $(nameField).val('');
                        $(priceField).val('');

                        if (options.onError && typeof options.onError === 'function') {
                            options.onError(response.message || 'Producto no encontrado');
                        }
                    }
                },
                error: function () {
                    App.notify.error('Error al buscar el producto');
                    self.productoActual = null;

                    if (options.onError && typeof options.onError === 'function') {
                        options.onError('Error de conexión');
                    }
                }
            });
        },

        // Agregar producto a una tabla
        addProductToTable: function (tableId, producto, cantidad, options) {
            options = options || {};
            const self = this;

            if (!producto || !producto.id) {
                App.notify.warning('Debe buscar un producto primero');
                return false;
            }

            const cantidadNum = parseInt(cantidad);
            if (isNaN(cantidadNum) || cantidadNum <= 0) {
                App.notify.warning('La cantidad debe ser mayor a cero');
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
                // Actualizar cantidad en fila existente
                const row = $(`#${tableId} tbody tr`).eq(index);
                const cantidadActual = parseInt(row.find('.cantidad').val());
                const nuevaCantidad = cantidadActual + cantidadNum;

                row.find('.cantidad').val(nuevaCantidad);
                row.find('.subtotal').text(App.format.currency(nuevaCantidad * producto.precio));
                row.find('input[name$=".PrecioTotal"]').val((nuevaCantidad * producto.precio).toFixed(2));
            } else {
                // Crear nueva fila
                const rowCount = $(`#${tableId} tbody tr`).length;
                const prefix = options.prefix || 'Productos';

                const newRow = `
                   <tr data-index="${rowCount}">
                       <td>
                           <input type="hidden" name="${prefix}[${rowCount}].ProductoID" value="${producto.id}" />
                           <input type="hidden" name="${prefix}[${rowCount}].CodigoAlfa" value="${producto.codigoAlfa}" />
                           <input type="hidden" name="${prefix}[${rowCount}].CodigoBarra" value="${producto.codigoBarra}" />
                           <input type="hidden" name="${prefix}[${rowCount}].Marca" value="${producto.marca}" />
                           <input type="hidden" name="${prefix}[${rowCount}].NombreProducto" value="${producto.nombre}" />
                           <input type="hidden" name="${prefix}[${rowCount}].PrecioUnitario" value="${producto.precio.toFixed(2)}" />
                           <input type="hidden" name="${prefix}[${rowCount}].PrecioTotal" value="${subtotal.toFixed(2)}" />
                           <input type="hidden" name="${prefix}[${rowCount}].PrecioLista" value="${producto.precioLista.toFixed(2)}" />
                           ${producto.codigoAlfa || producto.codigoBarra || producto.id}
                       </td>
                       <td>${producto.nombre}</td>
                       <td><input type="number" name="${prefix}[${rowCount}].Cantidad" value="${cantidadNum}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                       <td>${App.format.currency(producto.precio)}</td>
                       <td><span class="subtotal">${App.format.currency(subtotal)}</span></td>
                       <td class="text-center">
                           <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                               <i class="bi bi-trash"></i>
                           </button>
                       </td>
                   </tr>
               `;

                $(`#${tableId} tbody`).append(newRow);
            }

            // Actualizar totales
            if (options.updateTotals && typeof options.updateTotals === 'function') {
                options.updateTotals();
            } else if (options.autoUpdateTotals !== false) {
                App.tables.updateTotals(tableId, options.totalOptions);
            }

            // Limpiar campos
            if (options.resetFields !== false) {
                $('#productoCodigo, #productoNombre, #productoPrecio').val('');
            }

            return true;
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

        // Configurar filtros
        setupFilterHandlers: function () {
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

            // Cargar SubRubros al cambiar Rubro
            $(document).on('change', '.rubro-dropdown', function () {
                const rubroId = $(this).val();
                const subRubroDropdownId = $(this).data('subrubro-target') || 'subRubroDropdown';
                const currentSubRubroId = $('#' + subRubroDropdownId).data('current-value');

                App.productoController.loadSubRubros(rubroId, subRubroDropdownId, currentSubRubroId);
            });
        },

        // Cargar SubRubros para un Rubro dado
        loadSubRubros: function (rubroId, subRubroDropdownId, currentSubRubroId) {
            if (!rubroId) {
                $(`#${subRubroDropdownId}`).empty().append('<option value="">-- Seleccione SubRubro --</option>');
                return;
            }

            // Deshabilitar dropdown mientras carga
            $(`#${subRubroDropdownId}`).prop('disabled', true).empty().append('<option value="">Cargando...</option>');

            $.getJSON('/Productos/GetSubRubros', { rubroId: rubroId }, function (data) {
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

                App.notify.error("Error al cargar subrubros");
            });
        },

        // Configurar eventos para ajuste de precios
        setupAjustePreciosEvents: function () {
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
        },

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

        // Submódulo para ajuste de precios
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
                    App.productoController.precioAjuste.actualizarDuracion();
                });

                // Seleccionar/deseleccionar todos
                $(document).on('change', '#checkAll', function () {
                    const isChecked = $(this).prop('checked');
                    $('.producto-check').prop('checked', isChecked);
                });

                // Botón de simulación
                $(document).on('click', '#btnSimular', function () {
                    App.productoController.precioAjuste.simularAjuste();
                });
            },

            // Configuración para ajustes definitivos
            setupAjusteDefinitivo: function () {
                // Filtrar productos por criterios
                $(document).on('change', '#filtroRubro, #filtroMarca, #filtroSubRubro', function () {
                    App.productoController.precioAjuste.filtrarProductos();
                });

                // Aplicar ajuste a seleccionados
                $(document).on('click', '#btnAplicarAjuste', function () {
                    App.productoController.precioAjuste.aplicarAjuste();
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
                    FechaInicio: fechaInicio.toISOString(),
                    FechaFin: fechaFin.toISOString(),
                    TipoAjuste: tipoAjuste
                };

                // Enviar solicitud AJAX
                fetch('/AjustePrecios/SimularAjusteTemporal', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
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
                            App.productoController.precioAjuste.filtrarProductos();
                        }, 1000);
                    } else {
                        App.notify.error('Error al aplicar el ajuste: ' + response.message);
                    }
                });
            }
        }
    };

    // Para mantener compatibilidad con código existente
    App.products = {
        init: function () {
            // Inicializar controller principal
            if (!App.productoController._initialized) {
                App.productoController.init();
                App.productoController._initialized = true;
            }
        },

        // Métodos para compatibilidad
        searchByCode: function (url, code, options) {
            App.productoController.searchProductByCode(code, Object.assign({ url: url }, options));
        },

        addToTable: function (tableId, producto, cantidad, options) {
            return App.productoController.addProductToTable(tableId, producto, cantidad, options);
        },

        getProductRowTemplate: function (index, producto, cantidad, subtotal) {
            // Crear una fila temporal usando el método principal y extraer el HTML
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = App.productoController.addProductToTable('tempTable', producto, cantidad, {
                prefix: 'Productos',
                resetFields: false,
                autoUpdateTotals: false
            });

            // Limpiar el DOM temporal
            tempDiv.innerHTML = '';

            // Usar la implementación original para mantener compatibilidad exacta
            return `
               <tr data-index="${index}">
                   <td>
                       <input type="hidden" name="ProductosPresupuesto[${index}].ProductoID" value="${producto.id}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].CodigoAlfa" value="${producto.codigoAlfa || ''}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].CodigoBarra" value="${producto.codigoBarra || ''}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].Marca" value="${producto.marca || ''}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].NombreProducto" value="${producto.nombre}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].PrecioUnitario" value="${producto.precio}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].PrecioTotal" value="${subtotal}" />
                       <input type="hidden" name="ProductosPresupuesto[${index}].PrecioLista" value="${producto.precioLista || 0}" />
                       ${producto.codigoAlfa || producto.codigoBarra || producto.id}
                   </td>
                   <td>${producto.nombre}</td>
                   <td><input type="number" name="ProductosPresupuesto[${index}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                   <td>${App.format.currency(producto.precio)}</td>
                   <td><span class="subtotal">${App.format.currency(subtotal)}</span></td>
                   <td class="text-center">
                       <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                           <i class="bi bi-trash"></i>
                       </button>
                   </td>
               </tr>
           `;
        }
    };

    // Para mantener compatibilidad con código existente
    App.productosController = {
        init: function () {
            // Inicializar controller principal
            if (!App.productoController._initialized) {
                App.productoController.init();
                App.productoController._initialized = true;
            }
        },

        // Métodos compatibles
        setupActionHandlers: function () {
            App.productoController.setupEventHandlers();
        },

        loadSubRubros: function (rubroId, subRubroDropdownId, currentSubRubroId) {
            App.productoController.loadSubRubros(rubroId, subRubroDropdownId, currentSubRubroId);
        },

        calculateAdjustedPrices: function (productos, porcentaje, esAumento) {
            return App.productoController.calculateAdjustedPrices(productos, porcentaje, esAumento);
        }
    };

})(window, jQuery);