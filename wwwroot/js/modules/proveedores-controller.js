// proveedores-controller.js - Módulo unificado para proveedores y compras
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.proveedoresController = {
        // Variables compartidas
        assignedProducts: [], // Para proveedor-form
        productoActual: {     // Para compra-form
            id: 0,
            nombre: '',
            precio: 0
        },

        // INICIALIZACIÓN
        init: function () {
            console.log('Inicializando módulo unificado de proveedores');

            // Detectar en qué página estamos
            const isProveedoresList = $('#proveedoresTableBody').length > 0;
            const isProveedorForm = $('#assignedProducts').length > 0;
            const isCompraForm = $('#productosTable').length > 0 && $('#proveedorDropdown').length > 0;

            // Inicializar componentes según la página
            this.setupFilterHandlers();

            if (isProveedorForm) {
                this.loadAssignedProducts();
                this.setupProductSearch();
            }

            if (isCompraForm) {
                this.setupCompraForm();
            }
        },

        // CONTROLADOR GENERAL DE PROVEEDORES

        setupFilterHandlers: function () {
            // Filtrado de proveedores en vista Index
            $('#applyFilter').on('click', function () {
                const filters = {
                    filterField: $('#filterField').val(),
                    filterValue: $('#filterValue').val()
                };

                $.get('/Proveedores/Filter', filters)
                    .done(function (html) {
                        $('#proveedoresTableBody').html(html);
                    })
                    .fail(function () {
                        App.notify?.error('Error al aplicar filtros');
                    });
            });

            $('#filterValue').on('keypress', function (e) {
                if (e.which === 13) {
                    $('#applyFilter').click();
                    return false;
                }
            });
        },

        // Búsqueda de productos para asignación
        searchProductsForAssignment: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify?.warning('Ingrese al menos 2 caracteres para buscar');
                return;
            }

            $.ajax({
                url: '/Proveedores/SearchProductsForAssignment',
                method: 'POST',
                data: { term: term },
                success: function (data) {
                    if (callback && typeof callback === 'function') {
                        callback(data);
                    }
                },
                error: function () {
                    App.notify?.error('Error al buscar productos');
                }
            });
        },

        // Búsqueda avanzada de productos para compra
        searchProductsForPurchase: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify?.warning('Ingrese al menos 2 caracteres para buscar');
                return;
            }

            $.ajax({
                url: '/Proveedores/SearchProductsForPurchase',
                method: 'POST',
                data: { term: term },
                success: function (data) {
                    if (callback && typeof callback === 'function') {
                        callback(data);
                    }
                },
                error: function () {
                    App.notify?.error('Error al buscar productos');
                }
            });
        },

        // FORMULARIO DE PROVEEDOR

        // Cargar productos asignados iniciales
        loadAssignedProducts: function () {
            this.assignedProducts = [];

            $('#assignedProducts li, #assignedProducts tr').each(function () {
                const id = parseInt($(this).data('id'));
                if (!isNaN(id) && id > 0) {
                    App.proveedoresController.assignedProducts.push(id);
                }
            });
        },

        // Configurar búsqueda de productos
        setupProductSearch: function () {
            const self = this;

            // Búsqueda con autocomplete
            $('#productSearch').autocomplete({
                source: function (req, resp) {
                    $.getJSON('/Proveedores/SearchProducts', { term: req.term })
                        .done(resp)
                        .fail(function () {
                            App.notify?.error('Error al buscar productos');
                        });
                },
                select: function (e, ui) {
                    e.preventDefault();
                    const id = ui.item.value;
                    const name = ui.item.label;
                    const marca = ui.item.marca || '';

                    self.addProduct(id, name, marca);
                    $(this).val('');
                    return false;
                },
                minLength: 2
            });

            // Búsqueda con botón
            $('#searchProductBtn').on('click', function () {
                const searchTerm = $('#productSearch').val();

                self.searchProductsForAssignment(searchTerm, function (data) {
                    // Limpiar resultados anteriores
                    $('#productResultsTable tbody').empty();

                    if (data.length === 0) {
                        $('#productResultsTable tbody').append(
                            '<tr><td colspan="3" class="text-center">No se encontraron productos</td></tr>'
                        );
                    } else {
                        // Mostrar productos
                        data.forEach(function (product) {
                            const isAssigned = self.assignedProducts.includes(product.id);
                            const assignBtn = isAssigned ?
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
                });
            });

            // Asignar producto al hacer clic en el botón Asignar
            $(document).on('click', '.assign-product', function () {
                const id = $(this).data('id');
                const name = $(this).data('name');
                const marca = $(this).data('marca');

                self.addProduct(id, name, marca);

                // Actualizar botón en resultados
                $(this).replaceWith('<button type="button" class="btn btn-sm btn-secondary" disabled>Asignado</button>');
            });

            // Eliminar producto
            $(document).on('click', '.remove-product', function () {
                const row = $(this).closest('tr, li');
                const id = row.data('id');

                // Eliminar de la lista
                self.assignedProducts = self.assignedProducts.filter(function (pid) {
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
            $('#productSearch').keypress(function (e) {
                if (e.which === 13) {
                    $('#searchProductBtn').click();
                    return false;
                }
            });
        },

        // Añadir producto a la lista de asignados
        addProduct: function (id, name, marca) {
            id = parseInt(id);

            if (!this.assignedProducts.includes(id)) {
                this.assignedProducts.push(id);

                // Determinar dónde y cómo agregar el producto (lista o tabla)
                if ($('#assignedProducts').is('ul')) {
                    // Agregar a lista
                    $('#assignedProducts').append(
                        `<li class="list-group-item d-flex justify-content-between align-items-center" data-id="${id}">` +
                        `${name} <button type="button" class="btn btn-link btn-sm remove-product">Eliminar</button>` +
                        `<input type="hidden" name="ProductosAsignados" value="${id}" />` +
                        `</li>`);
                } else {
                    // Agregar a tabla
                    const rowCount = $('#assignedProducts tr').length;
                    $('#assignedProducts').append(
                        `<tr data-id="${id}">` +
                        `<td>${name}</td>` +
                        `<td>${marca}</td>` +
                        `<td>` +
                        `<button type="button" class="btn btn-sm btn-outline-danger remove-product">` +
                        `<i class="bi bi-trash"></i>` +
                        `</button>` +
                        `<input type="hidden" name="ProductosAsignados" value="${id}" />` +
                        `</td>` +
                        `</tr>`
                    );
                }
            }
        },

        // FORMULARIO DE COMPRA

        setupCompraForm: function () {
            console.log('Inicializando formulario de compra');
            this.setupCompraProductSearch();
            this.setupCompraFormaPago();
        },

        // Configurar búsqueda de productos en compra
        setupCompraProductSearch: function () {
            const self = this;

            // Colapsar/Expandir sección de búsqueda
            $('#toggleSearchBtn').on('click', function () {
                $('#searchProductSection').toggleClass('d-none');
                $(this).find('i').toggleClass('bi-arrows-expand bi-arrows-collapse');
            });

            // Búsqueda de producto por código
            $('#buscarProducto').on('click', function () {
                const codigo = $('#productoCodigo').val();
                if (!codigo) {
                    App.notify?.warning('Ingrese un código para buscar');
                    return;
                }

                $.ajax({
                    url: '/Proveedores/BuscarProducto',
                    type: 'POST',
                    data: { codigoProducto: codigo },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            // Guardar datos del producto
                            self.productoActual = {
                                id: response.data.productoID,
                                nombre: response.data.nombreProducto,
                                precio: response.data.precioUnitario
                            };

                            // Mostrar datos del producto
                            $('#productoNombre').val(self.productoActual.nombre);
                            $('#productoPrecio').val(self.productoActual.precio);
                            $('#productoCantidad').val(1);
                            $('#productoCantidad').focus();
                        } else {
                            // Mostrar modal de error
                            $('#productoNoEncontradoModal').modal('show');

                            // Limpiar campos
                            self.productoActual = { id: 0, nombre: '', precio: 0 };
                            $('#productoNombre, #productoPrecio').val('');
                        }
                    },
                    error: function () {
                        console.error('Error en la búsqueda del producto');
                        // Mostrar modal de error
                        $('#productoNoEncontradoModal').modal('show');
                    }
                });
            });

            // Búsqueda con Enter
            $('#productoCodigo').keypress(function (e) {
                if (e.which === 13) {
                    $('#buscarProducto').click();
                    return false;
                }
            });

            // Búsqueda en modal
            $('#modalSearchBtn').on('click', function () {
                const term = $('#modalSearchTerm').val();
                if (!term || term.length < 2) {
                    App.notify?.warning('Ingrese al menos 2 caracteres para buscar');
                    return;
                }

                $.ajax({
                    url: '/Proveedores/SearchProducts',
                    type: 'POST',
                    data: { term: term, forPurchase: true },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (data) {
                        // Limpiar resultados anteriores
                        const tbody = $('#modalResultsTable tbody');
                        tbody.empty();

                        if (!data || data.length === 0) {
                            tbody.append('<tr><td colspan="5" class="text-center">No se encontraron productos</td></tr>');
                        } else {
                            // Agregar resultados
                            data.forEach(function (p) {
                                const formattedPrice = new Intl.NumberFormat('es-AR', {
                                    style: 'currency',
                                    currency: 'ARS'
                                }).format(p.precio);

                                tbody.append(`
                                    <tr>
                                        <td>${p.codigo}</td>
                                        <td>${p.name}</td>
                                        <td>${p.marca}</td>
                                        <td>${formattedPrice}</td>
                                        <td class="text-center">
                                            <button type="button" class="btn btn-sm btn-primary select-product"
                                                    data-id="${p.id}" data-name="${p.name}" data-precio="${p.precio}">
                                                <i class="bi bi-plus-circle"></i> Seleccionar
                                            </button>
                                        </td>
                                    </tr>
                                `);
                            });
                        }
                    },
                    error: function () {
                        console.error('Error en la búsqueda de productos');
                    }
                });
            });

            // Al presionar Enter en el campo de búsqueda del modal
            $('#modalSearchTerm').on('keypress', function (e) {
                if (e.which === 13) {
                    $('#modalSearchBtn').click();
                    return false;
                }
            });

            // Seleccionar producto desde el modal
            $(document).on('click', '.select-product', function () {
                self.productoActual = {
                    id: $(this).data('id'),
                    nombre: $(this).data('name'),
                    precio: $(this).data('precio')
                };

                // Mostrar datos en el formulario
                $('#productoNombre').val(self.productoActual.nombre);
                $('#productoPrecio').val(self.productoActual.precio);
                $('#productoCantidad').val(1);

                // Cerrar modal
                $('#buscarProductosModal').modal('hide');
                $('#productoCantidad').focus();
            });

            // Agregar producto a la tabla
            $('#agregarProducto').on('click', function () {
                if (self.productoActual.id === 0) {
                    App.notify?.warning('Debe buscar un producto primero');
                    return;
                }

                const cantidad = parseInt($('#productoCantidad').val());
                const precio = parseFloat($('#productoPrecio').val());

                if (isNaN(cantidad) || cantidad <= 0) {
                    App.notify?.warning('La cantidad debe ser mayor a cero');
                    return;
                }

                if (isNaN(precio) || precio <= 0) {
                    App.notify?.warning('El precio debe ser mayor a cero');
                    return;
                }

                // Verificar si el producto ya está en la tabla
                let existe = false;
                let index = -1;

                $('#productosTable tbody tr').each(function (i) {
                    const productoID = $(this).find('input[name$=".ProductoID"]').val();
                    if (parseInt(productoID) === self.productoActual.id) {
                        existe = true;
                        index = i;
                        return false; // Salir del bucle
                    }
                });

                if (existe) {
                    // Actualizar cantidad
                    const cantidadActual = parseInt($('#productosTable tbody tr').eq(index).find('.cantidad').val());
                    const nuevaCantidad = cantidadActual + cantidad;
                    $('#productosTable tbody tr').eq(index).find('.cantidad').val(nuevaCantidad);

                    // Actualizar subtotal
                    const subtotal = nuevaCantidad * precio;
                    const subtotalFormatted = App.format?.currency ?
                        App.format.currency(subtotal) :
                        new Intl.NumberFormat('es-AR', {
                            style: 'currency',
                            currency: 'ARS'
                        }).format(subtotal);

                    $('#productosTable tbody tr').eq(index).find('.subtotal').text(subtotalFormatted);
                    $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(subtotal);
                } else {
                    // Crear nueva fila
                    const rowCount = $('#productosTable tbody tr').length;
                    const subtotal = cantidad * precio;

                    const subtotalFormatted = App.format?.currency ?
                        App.format.currency(subtotal) :
                        new Intl.NumberFormat('es-AR', {
                            style: 'currency',
                            currency: 'ARS'
                        }).format(subtotal);

                    const precioFormatted = App.format?.currency ?
                        App.format.currency(precio) :
                        new Intl.NumberFormat('es-AR', {
                            style: 'currency',
                            currency: 'ARS'
                        }).format(precio);

                    const newRow = `
                        <tr data-index="${rowCount}">
                            <td>
                                <input type="hidden" name="ProductosCompra[${rowCount}].ProductoID" value="${self.productoActual.id}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].NombreProducto" value="${self.productoActual.nombre}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].PrecioUnitario" value="${precio}" />
                                <input type="hidden" name="ProductosCompra[${rowCount}].PrecioTotal" value="${subtotal}" />
                                ${self.productoActual.id}
                            </td>
                            <td>${self.productoActual.nombre}</td>
                            <td><input type="number" name="ProductosCompra[${rowCount}].Cantidad" value="${cantidad}" min="1" class="form-control form-control-sm bg-dark text-light cantidad" /></td>
                            <td>${precioFormatted}</td>
                            <td><span class="subtotal">${subtotalFormatted}</span></td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-outline-danger eliminar-producto">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `;

                    $('#productosTable tbody').append(newRow);
                }

                // Actualizar totales
                self.updateCompraTotal();

                // Limpiar campos
                $('#productoCodigo, #productoNombre, #productoPrecio').val('');
                self.productoActual = { id: 0, nombre: '', precio: 0 };
            });

            // Eliminar producto
            $(document).on('click', '.eliminar-producto', function () {
                $(this).closest('tr').remove();
                self.updateCompraTotal();
                self.reindexCompraRows();
            });

            // Actualizar totales al cambiar cantidad
            $(document).on('change', '.cantidad', function () {
                const row = $(this).closest('tr');
                const cantidad = parseInt($(this).val());
                const precio = parseFloat(row.find('input[name$=".PrecioUnitario"]').val());
                const subtotal = cantidad * precio;

                const subtotalFormatted = App.format?.currency ?
                    App.format.currency(subtotal) :
                    new Intl.NumberFormat('es-AR', {
                        style: 'currency',
                        currency: 'ARS'
                    }).format(subtotal);

                row.find('.subtotal').text(subtotalFormatted);
                row.find('input[name$=".PrecioTotal"]').val(subtotal);

                self.updateCompraTotal();
            });
        },

        // Configurar forma de pago en compra
        setupCompraFormaPago: function () {
            if (!$('#FormaPagoID').length) return;

            $('#FormaPagoID').change(function () {
                const formaPagoID = parseInt($(this).val());

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor correspondiente
                switch (formaPagoID) {
                    case 2: // Tarjeta de Crédito
                        $('#tarjetaCreditoContainer').removeClass('d-none');
                        break;
                    case 3: // Tarjeta de Débito
                        $('#tarjetaDebitoContainer').removeClass('d-none');
                        break;
                    case 4: // Transferencia
                        $('#transferenciaContainer').removeClass('d-none');
                        break;
                    case 5: // Pago Virtual
                        $('#pagoVirtualContainer').removeClass('d-none');
                        break;
                    case 6: // Crédito Personal
                        $('#creditoPersonalContainer').removeClass('d-none');
                        break;
                    case 7: // Cheque
                        $('#chequeContainer').removeClass('d-none');
                        break;
                }
            });

            // Ejecutar cambio inicial para mostrar campos si ya hay forma de pago seleccionada
            $('#FormaPagoID').trigger('change');
        },

        // Actualizar totales de compra
        updateCompraTotal: function () {
            let totalProductos = 0;
            let totalCompra = 0;

            $('#productosTable tbody tr').each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                totalProductos += cantidad;
                totalCompra += subtotal;
            });

            $('#totalProductos').text(totalProductos);

            const totalCompraFormatted = App.format?.currency ?
                App.format.currency(totalCompra) :
                new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: 'ARS'
                }).format(totalCompra);

            $('#totalCompra').text(totalCompraFormatted);

            // Establecer campo oculto con el total
            if ($('#compraForm').length) {
                $('input[name="ImporteTotal"]').remove();
                $('<input>').attr({
                    type: 'hidden',
                    name: 'ImporteTotal',
                    value: totalCompra.toFixed(2)
                }).appendTo('#compraForm');
            }
        },

        // Reindexar filas de compra después de eliminar
        reindexCompraRows: function () {
            $('#productosTable tbody tr').each(function (index) {
                $(this).attr('data-index', index);

                $(this).find('input').each(function () {
                    const name = $(this).attr('name');
                    if (name && name.includes('[')) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            });
        }
    };

    // Alias para compatibilidad con código existente
    App.proveedorForm = {
        init: function () {
            App.proveedoresController.loadAssignedProducts();
            App.proveedoresController.setupProductSearch();
        },
        assignedProducts: App.proveedoresController.assignedProducts,
        addProduct: function (id, name, marca) {
            App.proveedoresController.addProduct(id, name, marca);
        }
    };

    App.compraForm = {
        init: function () {
            App.proveedoresController.setupCompraForm();
        },
        productoActual: App.proveedoresController.productoActual,
        updateTotals: function () {
            App.proveedoresController.updateCompraTotal();
        },
        reindexRows: function () {
            App.proveedoresController.reindexCompraRows();
        }
    };

})(window, jQuery);