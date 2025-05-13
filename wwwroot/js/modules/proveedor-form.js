// proveedor-form.js - Módulo para formulario de proveedores
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.proveedorForm = {
        assignedProducts: [],

        init: function () {
            this.loadAssignedProducts();
            this.setupProductSearch();
        },

        // Cargar productos asignados iniciales
        loadAssignedProducts: function () {
            this.assignedProducts = [];

            $('#assignedProducts li, #assignedProducts tr').each(function () {
                const id = parseInt($(this).data('id'));
                if (!isNaN(id) && id > 0) {
                    App.proveedorForm.assignedProducts.push(id);
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

                App.proveedoresController.searchProductsForAssignment(searchTerm, function (data) {
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
        }
    };

})(window, jQuery);