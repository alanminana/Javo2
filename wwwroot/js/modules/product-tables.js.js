// product-tables.js - Integración de tablas con productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.productTables = {
        // Producto actual para agregar a tablas
        currentProduct: null,

        init: function () {
            this.setupProductSearch();
            this.setupProductActions();
        },

        // Inicializar tabla de productos específica
        initProductTable: function (tableId, options) {
            options = options || {};

            // Configuración específica para tablas de productos
            const tableSettings = $.extend({
                prefix: 'Productos',
                updateTotalsOnChange: true,
                onQuantityChange: this.onProductQuantityChange,
                calculateTotals: true,
                totalProductsId: 'totalProductos',
                totalAmountId: 'totalVenta',
                productSearchSelector: '#productoCodigo',
                productSearchButton: '#buscarProducto',
                productNameField: '#productoNombre',
                productPriceField: '#productoPrecio',
                productQuantityField: '#productoCantidad',
                addProductButton: '#agregarProducto',
                productErrorModal: '#productoNoEncontradoModal'
            }, options);

            // Inicializar tabla mejorada
            const $table = App.enhancedTables.initTable(tableId, tableSettings);

            // Vincular eventos específicos de productos
            this.bindProductEventsToTable(tableId, tableSettings);

            return $table;
        },

        // Configurar búsqueda de productos
        setupProductSearch: function () {
            const self = this;

            // Botón de búsqueda de producto
            $(document).on('click', '.buscar-producto-btn', function () {
                const $searchInput = $(this).closest('.input-group').find('input').first();
                const codigo = $searchInput.val();

                if (!codigo) {
                    if (App.notify) {
                        App.notify.warning('Ingrese un código para buscar');
                    } else {
                        alert('Ingrese un código para buscar');
                    }
                    return;
                }

                // Opciones de búsqueda
                const options = {
                    nameField: $(this).data('name-field') || '#productoNombre',
                    priceField: $(this).data('price-field') || '#productoPrecio',
                    quantityField: $(this).data('quantity-field') || '#productoCantidad',
                    errorModal: $(this).data('error-modal') || '#productoNoEncontradoModal'
                };

                self.searchProductByCode(codigo, options);
            });

            // Enter en campo de búsqueda
            $(document).on('keypress', '.producto-codigo-input', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $(this).closest('.input-group').find('.buscar-producto-btn').click();
                }
            });
        },

        // Configurar acciones de productos
        setupProductActions: function () {
            const self = this;

            // Botón para agregar producto a tabla
            $(document).on('click', '.agregar-producto-btn', function () {
                const $btn = $(this);

                // Identificar tabla objetivo
                const tableId = $btn.data('table-id') || 'productosTable';

                // Opciones específicas
                const options = {
                    prefix: $btn.data('prefix') || 'Productos'
                };

                // Obtener producto actual
                let producto = self.currentProduct;

                // Si no hay producto, intentar obtenerlo del formulario
                if (!producto) {
                    const $form = $btn.closest('form');
                    producto = $form.data('productoActual');
                }

                if (!producto) {
                    if (App.notify) {
                        App.notify.warning('Debe buscar un producto primero');
                    } else {
                        alert('Debe buscar un producto primero');
                    }
                    return;
                }

                // Obtener cantidad
                const $cantidadInput = $('#' + ($btn.data('cantidad-field') || 'productoCantidad'));
                const cantidad = parseInt($cantidadInput.val());

                self.addProductToTable(tableId, producto, cantidad, options);
            });
        },

        // Búsqueda de producto por código
        searchProductByCode: function (codigo, options) {
            const self = this;
            options = options || {};

            const url = options.url || '/Productos/BuscarProducto';
            const nameField = options.nameField || '#productoNombre';
            const priceField = options.priceField || '#productoPrecio';
            const quantityField = options.quantityField || '#productoCantidad';
            const errorModal = options.errorModal || '#productoNoEncontradoModal';

            // Usar AJAX de App.common si está disponible
            if (App.ajax && App.ajax.post) {
                App.ajax.post(url, { codigoProducto: codigo }, function (response) {
                    self.handleProductSearchResponse(response, {
                        nameField: nameField,
                        priceField: priceField,
                        quantityField: quantityField,
                        errorModal: errorModal,
                        onSuccess: options.onSuccess,
                        onError: options.onError
                    });
                }, function () {
                    if (App.notify) {
                        App.notify.error('Error al buscar el producto');
                    } else {
                        alert('Error al buscar el producto');
                    }

                    if (options.onError) options.onError('Error de conexión');
                });
            } else {
                // AJAX básico si App.common no está disponible
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: { codigoProducto: codigo },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        self.handleProductSearchResponse(response, {
                            nameField: nameField,
                            priceField: priceField,
                            quantityField: quantityField,
                            errorModal: errorModal,
                            onSuccess: options.onSuccess,
                            onError: options.onError
                        });
                    },
                    error: function () {
                        if (App.notify) {
                            App.notify.error('Error al buscar el producto');
                        } else {
                            alert('Error al buscar el producto');
                        }

                        if (options.onError) options.onError('Error de conexión');
                    }
                });
            }
        },

        // Manejar respuesta de búsqueda de producto
        handleProductSearchResponse: function (response, options) {
            if (response.success) {
                this.currentProduct = {
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
                $(options.nameField).val(this.currentProduct.nombre);
                $(options.priceField).val(this.currentProduct.precio.toFixed(2));

                if (options.quantityField) {
                    $(options.quantityField).val(1).focus();
                }

                // Almacenar en el formulario para acceso fácil
                const $form = $(options.nameField).closest('form');
                if ($form.length) {
                    $form.data('productoActual', this.currentProduct);
                }

                // Callback de éxito
                if (options.onSuccess) options.onSuccess(this.currentProduct);
            } else {
                // Mostrar error
                $(options.errorModal).modal('show');
                this.currentProduct = null;

                // Limpiar campos
                $(options.nameField).val('');
                $(options.priceField).val('');

                // Callback de error
                if (options.onError) options.onError(response.message || 'Producto no encontrado');
            }
        },

        // Agregar producto a una tabla
        addProductToTable: function (tableId, producto, cantidad, options) {
            options = options || {};

            if (!producto || !producto.id) {
                if (App.notify) {
                    App.notify.warning('Debe buscar un producto primero');
                } else {
                    alert('Debe buscar un producto primero');
                }
                return false;
            }

            const cantidadNum = parseInt(cantidad);
            if (isNaN(cantidadNum) || cantidadNum <= 0) {
                if (App.notify) {
                    App.notify.warning('La cantidad debe ser mayor a cero');
                } else {
                    alert('La cantidad debe ser mayor a cero');
                }
                return false;
            }

            const $table = $('#' + tableId);
            const prefix = options.prefix || 'Productos';
            const subtotal = cantidadNum * producto.precio;

            // Verificar si el producto ya está en la tabla
            let existe = false;
            let $existingRow = null;

            $(`#${tableId} tbody tr`).each(function () {
                const productoID = $(this).find(`input[name$=".ProductoID"]`).val();
                if (parseInt(productoID) === producto.id) {
                    existe = true;
                    $existingRow = $(this);
                    return false; // Salir del bucle
                }
            });

            if (existe && $existingRow) {
                // Actualizar cantidad en fila existente
                const cantidadActual = parseInt($existingRow.find('.cantidad').val());
                const nuevaCantidad = cantidadActual + cantidadNum;

                $existingRow.find('.cantidad').val(nuevaCantidad);

                // Calcular nuevo subtotal
                const nuevoPrecio = producto.precio; // Usar precio actualizado si ha cambiado
                const nuevoSubtotal = nuevaCantidad * nuevoPrecio;

                // Actualizar datos en la fila
                $existingRow.find('input[name$=".PrecioUnitario"]').val(nuevoPrecio.toFixed(2));
                $existingRow.find('.subtotal').text(App.format ? App.format.currency(nuevoSubtotal) : nuevoSubtotal.toFixed(2));
                $existingRow.find('input[name$=".PrecioTotal"]').val(nuevoSubtotal.toFixed(2));
            } else {
                // Preparar datos para la nueva fila
                const rowIndex = $table.find('tbody tr').length;

                // Generar fila HTML
                const formattedPrice = App.format ? App.format.currency(producto.precio) : producto.precio.toFixed(2);
                const formattedSubtotal = App.format ? App.format.currency(subtotal) : subtotal.toFixed(2);

                const newRow = `
                <tr data-index="${rowIndex}">
                    <td>
                        <input type="hidden" name="${prefix}[${rowIndex}].ProductoID" value="${producto.id}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].CodigoAlfa" value="${producto.codigoAlfa}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].CodigoBarra" value="${producto.codigoBarra}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].Marca" value="${producto.marca}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].NombreProducto" value="${producto.nombre}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].PrecioUnitario" value="${producto.precio.toFixed(2)}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].PrecioTotal" value="${subtotal.toFixed(2)}" />
                        <input type="hidden" name="${prefix}[${rowIndex}].PrecioLista" value="${producto.precioLista.toFixed(2)}" />
                        ${producto.codigoAlfa || producto.codigoBarra || producto.id}
                    </td>
                    <td>${producto.nombre}</td>
                    <td><input type="number" name="${prefix}[${rowIndex}].Cantidad" value="${cantidadNum}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                    <td>${formattedPrice}</td>
                    <td><span class="subtotal">${formattedSubtotal}</span></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>`;

                // Añadir fila a la tabla
                $table.find('tbody').append(newRow);
            }

            // Actualizar totales
            App.enhancedTables.updateTotals(tableId, {
                totalProductsId: options.totalProductsId || 'totalProductos',
                totalAmountId: options.totalAmountId || 'totalVenta',
                hiddenTotalInput: options.hiddenTotalInput
            });

            // Limpiar campos de búsqueda
            if (options.resetFields !== false) {
                $('#productoCodigo, #productoNombre, #productoPrecio').val('');
                this.currentProduct = null;
            }

            return true;
        },

        // Evento al cambiar cantidad de un producto
        onProductQuantityChange: function ($row, cantidad, precio, subtotal) {
            // Actualizar subtotal formateado si App.format está disponible
            if (App.format && App.format.currency) {
                $row.find('.subtotal').text(App.format.currency(subtotal));
            }
        },

        // Vincular eventos específicos de productos a una tabla
        bindProductEventsToTable: function (tableId, settings) {
            const self = this;
            const $table = $('#' + tableId);

            if (!$table.length) return;

            // Configurar búsqueda de producto
            const $searchButton = $(settings.productSearchButton);
            const $searchInput = $(settings.productSearchSelector);

            // Configurar búsqueda de producto
            const $searchButton = $(settings.productSearchButton);
            const $searchInput = $(settings.productSearchSelector);

            if ($searchButton.length && $searchInput.length) {
                $searchButton.off('click').on('click', function () {
                    const codigo = $searchInput.val();

                    self.searchProductByCode(codigo, {
                        nameField: settings.productNameField,
                        priceField: settings.productPriceField,
                        quantityField: settings.productQuantityField,
                        errorModal: settings.productErrorModal
                    });
                });

                // Enter en campo de búsqueda
                $searchInput.off('keypress').on('keypress', function (e) {
                    if (e.which === 13) {
                        e.preventDefault();
                        $searchButton.click();
                    }
                });
            }

            // Configurar botón de agregar producto
            const $addButton = $(settings.addProductButton);

            if ($addButton.length) {
                $addButton.off('click').on('click', function (e) {
                    e.preventDefault();

                    // Obtener producto actual
                    let producto = self.currentProduct;

                    // Si no hay producto, intentar obtenerlo del formulario
                    if (!producto) {
                        const $form = $addButton.closest('form');
                        producto = $form.data('productoActual');
                    }

                    if (!producto) {
                        if (App.notify) {
                            App.notify.warning('Debe buscar un producto primero');
                        } else {
                            alert('Debe buscar un producto primero');
                        }
                        return;
                    }

                    // Obtener cantidad
                    const cantidad = parseInt($(settings.productQuantityField).val());

                    // Agregar producto a la tabla
                    self.addProductToTable(tableId, producto, cantidad, {
                        prefix: settings.prefix,
                        totalProductsId: settings.totalProductsId,
                        totalAmountId: settings.totalAmountId,
                        resetFields: true
                    });
                });
            }
        }
    };

})(window, jQuery);