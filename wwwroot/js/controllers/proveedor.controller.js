// proveedor.controller.js - Controlador unificado para proveedores
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.proveedorController = {
        assignedProducts: [],

        init: function () {
            this.setupFilterHandlers();
            this.setupProductSearch();
            this.loadAssignedProducts();
        },

        // Cargar productos asignados iniciales
        loadAssignedProducts: function () {
            this.assignedProducts = [];

            $('#assignedProducts li, #assignedProducts tr').each(function () {
                const id = parseInt($(this).data('id'));
                if (!isNaN(id) && id > 0) {
                    App.proveedorController.assignedProducts.push(id);
                }
            });
        },

        // Configurar filtros de búsqueda
        setupFilterHandlers: function () {
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
                        App.notify.error('Error al aplicar filtros');
                    });
            });

            $('#filterValue').on('keypress', function (e) {
                if (e.which === 13) {
                    $('#applyFilter').click();
                    return false;
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
                            App.notify.error('Error al buscar productos');
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
                self.searchProductsForAssignment(searchTerm);
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

        // Búsqueda de productos para asignación
        searchProductsForAssignment: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify.warning('Ingrese al menos 2 caracteres para buscar');
                return;
            }

            $.ajax({
                url: '/Proveedores/SearchProductsForAssignment',
                method: 'POST',
                data: { term: term },
                success: (data) => {
                    // Si se proporcionó un callback, usarlo
                    if (callback && typeof callback === 'function') {
                        callback(data);
                        return;
                    }

                    // Procesamiento por defecto
                    // Limpiar resultados anteriores
                    $('#productResultsTable tbody').empty();

                    if (data.length === 0) {
                        $('#productResultsTable tbody').append(
                            '<tr><td colspan="3" class="text-center">No se encontraron productos</td></tr>'
                        );
                    } else {
                        // Mostrar productos
                        data.forEach((product) => {
                            const isAssigned = this.assignedProducts.includes(product.id);
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
                },
                error: function () {
                    App.notify.error('Error al buscar productos');
                }
            });
        },

        // Búsqueda avanzada de productos para compra
        searchProductsForPurchase: function (term, callback) {
            if (!term || term.length < 2) {
                App.notify.warning('Ingrese al menos 2 caracteres para buscar');
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
                    App.notify.error('Error al buscar productos');
                }
            });
        },

        // Módulo para formulario de compras
        compraForm: {
            productoActual: {
                id: 0,
                nombre: '',
                precio: 0
            },

            init: function () {
                this.setupProductSearch();
                this.setupFormaPago();
            },

            // Configurar búsqueda de productos
            setupProductSearch: function () {
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
                        App.notify.warning('Ingrese un código para buscar');
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
                        App.notify.warning('Ingrese al menos 2 caracteres para buscar');
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
                                    const formattedPrice = App.format.currency(p.precio);

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
                        App.notify.warning('Debe buscar un producto primero');
                        return;
                    }

                    const cantidad = parseInt($('#productoCantidad').val());
                    const precio = parseFloat($('#productoPrecio').val());

                    if (isNaN(cantidad) || cantidad <= 0) {
                        App.notify.warning('La cantidad debe ser mayor a cero');
                        return;
                    }

                    if (isNaN(precio) || precio <= 0) {
                        App.notify.warning('El precio debe ser mayor a cero');
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
                        const subtotalFormatted = App.format.currency(subtotal);

                        $('#productosTable tbody tr').eq(index).find('.subtotal').text(subtotalFormatted);
                        $('#productosTable tbody tr').eq(index).find('input[name$=".PrecioTotal"]').val(subtotal);
                    } else {
                        // Crear nueva fila
                        const rowCount = $('#productosTable tbody tr').length;
                        const subtotal = cantidad * precio;
                        const subtotalFormatted = App.format.currency(subtotal);

                        const precioFormatted = App.format.currency(precio);

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
                    self.updateTotals();

                    // Limpiar campos
                    $('#productoCodigo, #productoNombre, #productoPrecio').val('');
                    self.productoActual = { id: 0, nombre: '', precio: 0 };
                });

                // Eliminar producto
                $(document).on('click', '.eliminar-producto', function () {
                    $(this).closest('tr').remove();
                    self.updateTotals();
                    self.reindexRows();
                });

                // Actualizar totales al cambiar cantidad
                $(document).on('change', '.cantidad', function () {
                    const row = $(this).closest('tr');
                    const cantidad = parseInt($(this).val());
                    const precio = parseFloat(row.find('input[name$=".PrecioUnitario"]').val());
                    const subtotal = cantidad * precio;

                    const subtotalFormatted = App.format.currency(subtotal);

                    row.find('.subtotal').text(subtotalFormatted);
                    row.find('input[name$=".PrecioTotal"]').val(subtotal);

                    self.updateTotals();
                });
            },

            // Configurar forma de pago
            setupFormaPago: function () {
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

                // Ejecutar cambio inicial
                $('#FormaPagoID').trigger('change');
            },

            // Actualizar totales
            updateTotals: function () {
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
                $('#totalCompra').text(App.format.currency(totalCompra));
            },

            // Reindexar filas después de eliminar
            reindexRows: function () {
                $('#productosTable tbody tr').each(function (index) {
                    $(this).attr('data-index', index);

                    $(this).find('input').each(function () {
                        const name = $(this).attr('name');
                        if (name) {
                            const newName = name.replace(/\[\d+\]/, `[${index}]`);
                            $(this).attr('name', newName);
                        }
                    });
                });
            }
        }
    };

    // Para mantener compatibilidad con código existente
    App.proveedoresController = App.proveedorController;
    App.proveedorForm = App.proveedorController;
    App.compraForm = App.proveedorController.compraForm;

})(window, jQuery);