// enhanced-tables.js - Módulo mejorado para operaciones con tablas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.enhancedTables = {
        // Configuraciones por defecto
        defaults: {
            selectable: false,
            deletable: true,
            editable: true,
            reindexOnDelete: true,
            updateTotalsOnChange: true,
            confirmDelete: true,
            confirmDeleteMessage: '¿Está seguro que desea eliminar este elemento?',
            onDelete: null,
            onQuantityChange: null,
            onSelectionChange: null
        },

        // Inicializar el módulo
        init: function (options) {
            this.settings = $.extend({}, this.defaults, options);

            // Inicializar eventos básicos
            this.initDeleteButton();
            this.initQuantityChange();

            // Inicializar eventos adicionales si corresponde
            if (this.settings.selectable) {
                this.initSelectableRows();
            }

            return this;
        },

        // Inicializar tabla específica con opciones personalizadas
        initTable: function (tableId, options) {
            const tableSettings = $.extend({}, this.defaults, options);
            const $table = $('#' + tableId);

            if (!$table.length) {
                console.error('Tabla no encontrada:', tableId);
                return;
            }

            // Almacenar configuración en la tabla
            $table.data('table-settings', tableSettings);

            // Inicializar buscador si existe
            const $searchInput = $('#' + tableId + '-search');
            if ($searchInput.length) {
                this.initTableSearch(tableId, $searchInput);
            }

            // Inicializar selección si está habilitada
            if (tableSettings.selectable) {
                this.initSelectAll(tableId + '-select-all', '.' + tableId + '-select-item');
            }

            return $table;
        },

        // Configurar botón de eliminar fila
        initDeleteButton: function () {
            const self = this;

            $(document).on('click', '.eliminar-producto, .remove-product, .delete-row', function (e) {
                e.preventDefault();

                const $row = $(this).closest('tr');
                const $table = $row.closest('table');
                const tableId = $table.attr('id');
                const tableSettings = $table.data('table-settings') || self.settings;

                // Confirmar eliminación si está configurado
                if (tableSettings.confirmDelete) {
                    if (!confirm(tableSettings.confirmDeleteMessage)) {
                        return;
                    }
                }

                // Ejecutar callback antes de eliminar si existe
                if (typeof tableSettings.onDelete === 'function') {
                    const result = tableSettings.onDelete($row, $table);
                    if (result === false) return; // Cancelar eliminación si el callback devuelve false
                }

                // Eliminar fila
                $row.remove();

                // Actualizar tabla
                if (tableId) {
                    if (tableSettings.reindexOnDelete) {
                        self.reindexRows(tableId);
                    }

                    if (tableSettings.updateTotalsOnChange) {
                        self.updateTotals(tableId);
                    }
                }
            });
        },

        // Configurar cambio de cantidad
        initQuantityChange: function () {
            const self = this;

            $(document).on('change', '.cantidad', function () {
                const $input = $(this);
                const $row = $input.closest('tr');
                const $table = $row.closest('table');
                const tableId = $table.attr('id');
                const tableSettings = $table.data('table-settings') || self.settings;

                const cantidad = parseInt($input.val()) || 0;
                const precio = parseFloat($row.find('input[name$=".PrecioUnitario"]').val() || $row.data('precio')) || 0;
                const subtotal = cantidad * precio;

                // Actualizar subtotal en la fila
                $row.find('.subtotal').text(App.format ? App.format.currency(subtotal) : subtotal.toFixed(2));
                $row.find('input[name$=".PrecioTotal"]').val(subtotal.toFixed(2));

                // Ejecutar callback si existe
                if (typeof tableSettings.onQuantityChange === 'function') {
                    tableSettings.onQuantityChange($row, cantidad, precio, subtotal);
                }

                // Actualizar totales si corresponde
                if (tableId && tableSettings.updateTotalsOnChange) {
                    self.updateTotals(tableId);
                }
            });
        },

        // Inicializar filas seleccionables
        initSelectableRows: function () {
            $(document).on('click', '.selectable-row', function (e) {
                // Solo manejar clicks fuera de botones, links e inputs
                if (!$(e.target).is('button, a, input, .btn, .form-control')) {
                    const $row = $(this);
                    $row.toggleClass('selected');

                    // Actualizar checkbox si existe
                    const $checkbox = $row.find('input[type="checkbox"]');
                    if ($checkbox.length) {
                        $checkbox.prop('checked', $row.hasClass('selected'));
                    }
                }
            });
        },

        // Toggle seleccionar todo
        initSelectAll: function (checkAllSelector, itemSelector) {
            const self = this;

            $(document).on('change', checkAllSelector, function () {
                const isChecked = $(this).prop('checked');
                $(itemSelector).prop('checked', isChecked);

                // Resaltar filas si es necesario
                if ($(itemSelector).closest('tr').length) {
                    $(itemSelector).closest('tr').toggleClass('table-active', isChecked);
                }

                // Lanzar evento de cambio de selección
                const $table = $(this).closest('table');
                const tableSettings = $table.data('table-settings') || self.settings;

                if (typeof tableSettings.onSelectionChange === 'function') {
                    const selectedRows = $(itemSelector + ':checked').closest('tr');
                    tableSettings.onSelectionChange(selectedRows, $table);
                }
            });

            // Cambio de checkbox individual
            $(document).on('change', itemSelector, function () {
                const $table = $(this).closest('table');
                const tableId = $table.attr('id');
                const checkAllSelector = '#' + tableId + '-select-all';

                const allChecked = $(itemSelector).length === $(itemSelector + ':checked').length;
                $(checkAllSelector).prop('checked', allChecked);

                // Resaltar fila
                $(this).closest('tr').toggleClass('table-active', this.checked);

                // Lanzar evento de cambio de selección
                const tableSettings = $table.data('table-settings') || self.settings;

                if (typeof tableSettings.onSelectionChange === 'function') {
                    const selectedRows = $(itemSelector + ':checked').closest('tr');
                    tableSettings.onSelectionChange(selectedRows, $table);
                }
            });
        },

        // Inicializar búsqueda en tabla
        initTableSearch: function (tableId, $searchInput) {
            const $table = $('#' + tableId);

            $searchInput.on('input', function () {
                const searchTerm = $(this).val().toLowerCase();

                if (!searchTerm) {
                    // Mostrar todas las filas si no hay término de búsqueda
                    $table.find('tbody tr').show();
                    return;
                }

                // Filtrar filas
                $table.find('tbody tr').each(function () {
                    const $row = $(this);
                    const rowText = $row.text().toLowerCase();

                    if (rowText.indexOf(searchTerm) > -1) {
                        $row.show();
                    } else {
                        $row.hide();
                    }
                });
            });

            // Limpiar búsqueda
            const $clearButton = $searchInput.siblings('.clear-search');
            if ($clearButton.length) {
                $clearButton.on('click', function () {
                    $searchInput.val('').trigger('input');
                });
            }
        },

        // Reindexar filas para envío del formulario
        reindexRows: function (tableId) {
            $(`#${tableId} tbody tr`).each(function (index) {
                $(this).attr('data-index', index);

                // Buscar input fields con índices en el nombre
                $(this).find('input, select, textarea').each(function () {
                    const name = $(this).attr('name');
                    if (name && name.includes('[')) {
                        // Actualizar índice: Productos[0] -> Productos[nuevo_index]
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            });
        },

        // Actualizar totales
        updateTotals: function (tableId, options) {
            options = options || {};

            let totalProducts = 0;
            let totalAmount = 0;

            $(`#${tableId} tbody tr:visible`).each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val() || $(this).data('precio')) || 0;
                const subtotal = cantidad * precio;

                totalProducts += cantidad;
                totalAmount += subtotal;

                // Actualizar campo oculto para envío del formulario
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal.toFixed(2));
            });

            // Actualizar totales en la UI
            const totalProductsId = options.totalProductsId || tableId + '-total-products';
            const totalAmountId = options.totalAmountId || tableId + '-total-amount';

            $(`#${totalProductsId}`).text(totalProducts);

            // Usar formateo de moneda si está disponible
            if (App.format && App.format.currency) {
                $(`#${totalAmountId}`).text(App.format.currency(totalAmount));
            } else {
                $(`#${totalAmountId}`).text(totalAmount.toFixed(2));
            }

            // Establecer total oculto si se proporciona
            if (options.hiddenTotalInput) {
                $(options.hiddenTotalInput).val(totalAmount.toFixed(2));
            }

            // Devolver los totales
            return {
                products: totalProducts,
                amount: totalAmount
            };
        },

        // Añadir fila a una tabla
        addRow: function (tableId, rowData, options) {
            options = options || {};
            const $table = $('#' + tableId);

            if (!$table.length) {
                console.error('Tabla no encontrada:', tableId);
                return;
            }

            // Obtener índice para la nueva fila
            const rowIndex = options.index || $table.find('tbody tr').length;

            // Generar HTML de la fila
            let rowHtml = '<tr data-index="' + rowIndex + '">';

            // Si hay plantilla de fila, usarla
            if (options.template) {
                rowHtml = options.template
                    .replace(/\${index}/g, rowIndex)
                    .replace(/\${([^}]+)}/g, function (match, prop) {
                        return rowData[prop] || '';
                    });
            } else {
                // Generar celdas basadas en los datos
                for (const key in rowData) {
                    rowHtml += '<td>' + rowData[key] + '</td>';
                }

                // Añadir celda de acciones si es necesario
                if (options.actions) {
                    rowHtml += '<td class="text-center">' + options.actions + '</td>';
                }
            }

            rowHtml += '</tr>';

            // Añadir fila a la tabla
            const $newRow = $(rowHtml);
            $table.find('tbody').append($newRow);

            // Actualizar totales si es necesario
            if (options.updateTotals) {
                this.updateTotals(tableId);
            }

            return $newRow;
        },

        // Obtener datos de una tabla
        getTableData: function (tableId, options) {
            options = options || {};
            const $table = $('#' + tableId);
            const result = [];

            if (!$table.length) {
                console.error('Tabla no encontrada:', tableId);
                return result;
            }

            // Determinar qué filas procesar
            const selector = options.selector || 'tbody tr';

            $table.find(selector).each(function () {
                const rowData = {};

                // Capturar datos de inputs
                $(this).find('input, select, textarea').each(function () {
                    const $input = $(this);
                    const name = $input.attr('name');

                    if (name) {
                        // Extraer el nombre del campo: Productos[0].Nombre -> Nombre
                        const fieldMatch = name.match(/\.([^.]+)$/);
                        if (fieldMatch) {
                            const fieldName = fieldMatch[1];

                            // Capturar valor según tipo de input
                            if ($input.is(':checkbox')) {
                                rowData[fieldName] = $input.prop('checked');
                            } else {
                                rowData[fieldName] = $input.val();
                            }
                        }
                    }
                });

                // Si hay campos adicionales en data attributes
                if (options.includeDataAttributes) {
                    $.each(this.dataset, function (key, value) {
                        if (!rowData[key]) {
                            rowData[key] = value;
                        }
                    });
                }

                // Añadir datos de la fila al resultado
                result.push(rowData);
            });

            return result;
        },

        // Limpiar tabla
        clearTable: function (tableId) {
            const $table = $('#' + tableId);

            if (!$table.length) {
                console.error('Tabla no encontrada:', tableId);
                return;
            }

            // Eliminar todas las filas
            $table.find('tbody').empty();

            // Resetear totales
            this.updateTotals(tableId);

            return $table;
        }
    };

})(window, jQuery);