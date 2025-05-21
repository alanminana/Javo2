// forms.js - Módulo unificado para manejo de formularios y tablas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.forms = {
        // INICIALIZACIÓN

        init: function () {
            console.log('Inicializando módulo unificado de formularios y tablas');

            // Inicializar componentes de formularios
            this.initPaymentForms();

            // Inicializar componentes de tablas
            this.initDeleteButton();
            this.initQuantityChange();
        },

        // COMPONENTES DE FORMULARIOS

        // Inicializar formularios de pago
        initPaymentForms: function () {
            $(document).on('change', '.payment-toggle', function () {
                const formaPagoID = parseInt($(this).val());

                // Ocultar todos los contenedores
                $('.payment-container').addClass('d-none');

                // Mostrar el contenedor seleccionado
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

            // Trigger inicial
            $('.payment-toggle').trigger('change');
        },

        // Validar campos requeridos
        validateRequired: function (formSelector) {
            let isValid = true;

            $(formSelector + ' [required]').each(function () {
                const $field = $(this);
                const value = $field.val();

                if (!value || value.trim() === '') {
                    isValid = false;
                    $field.addClass('is-invalid');

                    // Verificar si ya existe el mensaje de error
                    let $errorMsg = $field.next('.invalid-feedback');
                    if ($errorMsg.length === 0) {
                        $errorMsg = $('<div class="invalid-feedback">Este campo es obligatorio</div>');
                        $field.after($errorMsg);
                    }

                    $errorMsg.show();
                } else {
                    $field.removeClass('is-invalid');
                    $field.next('.invalid-feedback').hide();
                }
            });

            return isValid;
        },

        // Serializar formulario a JSON
        serializeToJson: function (formSelector) {
            const formData = $(formSelector).serializeArray();
            const jsonData = {};

            $.each(formData, function () {
                if (jsonData[this.name]) {
                    if (!jsonData[this.name].push) {
                        jsonData[this.name] = [jsonData[this.name]];
                    }
                    jsonData[this.name].push(this.value || '');
                } else {
                    jsonData[this.name] = this.value || '';
                }
            });

            return jsonData;
        },

        // Validación de contraseñas
        setupPasswordStrength: function (passwordInput, feedbackContainer) {
            $(passwordInput).on('input', function () {
                const valor = $(this).val();
                const validaciones = {
                    longitud: valor.length >= 6,
                    letraNumero: /[a-zA-Z]/.test(valor) && /[0-9]/.test(valor),
                    caracterEspecial: /[^a-zA-Z0-9]/.test(valor)
                };

                let html = '<div class="fw-bold mb-1">Fortaleza de la contraseña:</div>';
                html += '<ul class="mb-0 ps-3">';
                html += `<li class="${validaciones.longitud ? 'text-success' : 'text-danger'}">Al menos 6 caracteres</li>`;
                html += `<li class="${validaciones.letraNumero ? 'text-success' : 'text-danger'}">Letras y números</li>`;
                html += `<li class="${validaciones.caracterEspecial ? 'text-success' : 'text-danger'}">Al menos un carácter especial</li>`;
                html += '</ul>';

                $(feedbackContainer).html(html);
            });
        },

        // Configurar validación de entrada numérica
        setupNumericInput: function (selector, options) {
            options = options || {};
            const allowDecimals = options.allowDecimals !== undefined ? options.allowDecimals : true;
            const allowNegative = options.allowNegative !== undefined ? options.allowNegative : false;
            const maxValue = options.maxValue !== undefined ? options.maxValue : null;
            const minValue = options.minValue !== undefined ? options.minValue : null;

            $(selector).on('input', function () {
                let value = $(this).val();

                // Eliminar caracteres no permitidos
                if (allowDecimals) {
                    // Permitir números y punto decimal
                    value = value.replace(/[^0-9.-]/g, '');

                    // Asegurar solo un punto decimal
                    const parts = value.split('.');
                    if (parts.length > 2) {
                        value = parts[0] + '.' + parts.slice(1).join('');
                    }
                } else {
                    // Solo números enteros
                    value = value.replace(/[^0-9-]/g, '');
                }

                // Manejar signo negativo
                if (!allowNegative) {
                    value = value.replace(/-/g, '');
                } else if (value.indexOf('-') > 0) {
                    // El signo negativo solo puede estar al principio
                    value = value.replace(/-/g, '');
                    if (value.charAt(0) !== '-') {
                        value = '-' + value;
                    }
                }

                // Aplicar restricciones de valor mínimo/máximo
                const numValue = parseFloat(value);
                if (!isNaN(numValue)) {
                    if (maxValue !== null && numValue > maxValue) {
                        value = maxValue.toString();
                    }
                    if (minValue !== null && numValue < minValue) {
                        value = minValue.toString();
                    }
                }

                // Actualizar valor si cambió
                if (value !== $(this).val()) {
                    $(this).val(value);
                }
            });

            // Validación en cambio (cuando se pierde el foco)
            $(selector).on('change', function () {
                const value = $(this).val();

                // Si está vacío y es requerido, manejar según opciones
                if (value === '' && options.required) {
                    $(this).val(options.defaultValue || '0');
                }

                // Si es un número inválido, usar valor por defecto
                if (value !== '' && isNaN(parseFloat(value))) {
                    $(this).val(options.defaultValue || '0');
                }
            });
        },

        // Inicializar formulario con validación personalizada
        initCustomForm: function (formSelector, options) {
            options = options || {};
            const self = this;

            const $form = $(formSelector);
            if (!$form.length) return;

            // Configurar validación en submit
            $form.on('submit', function (e) {
                // Validación básica de campos requeridos
                let isValid = self.validateRequired(formSelector);

                // Validaciones personalizadas adicionales
                if (isValid && options.customValidation && typeof options.customValidation === 'function') {
                    isValid = options.customValidation($form);
                }

                // Si hay errores, prevenir envío
                if (!isValid) {
                    e.preventDefault();
                    e.stopPropagation();

                    // Scroll al primer error
                    const $firstError = $form.find('.is-invalid').first();
                    if ($firstError.length) {
                        $('html, body').animate({
                            scrollTop: $firstError.offset().top - 100
                        }, 200);
                    }

                    // Mostrar mensaje de error si se proporciona
                    if (options.errorContainer) {
                        $(options.errorContainer).removeClass('d-none')
                            .html(options.errorMessage || 'Por favor corrija los errores en el formulario.');
                    }

                    return false;
                }

                // Callback antes del envío
                if (options.beforeSubmit && typeof options.beforeSubmit === 'function') {
                    const result = options.beforeSubmit($form);
                    if (result === false) {
                        e.preventDefault();
                        e.stopPropagation();
                        return false;
                    }
                }

                // Deshabilitar botón de envío para prevenir doble click
                if (options.disableSubmitButton !== false) {
                    const $submitBtn = $form.find('[type="submit"]');
                    if ($submitBtn.length) {
                        $submitBtn.prop('disabled', true);
                        if (options.submitButtonLoadingText) {
                            const originalText = $submitBtn.text();
                            $submitBtn.data('original-text', originalText);
                            $submitBtn.html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${options.submitButtonLoadingText}`);
                        }
                    }
                }

                return true;
            });

            // Inicializar validaciones en tiempo real si se solicita
            if (options.liveValidation) {
                $form.find('[required], [data-validate]').on('blur', function () {
                    const $field = $(this);
                    const value = $field.val();

                    if (!value || value.trim() === '') {
                        $field.addClass('is-invalid');

                        // Verificar si ya existe el mensaje de error
                        let $errorMsg = $field.next('.invalid-feedback');
                        if ($errorMsg.length === 0) {
                            $errorMsg = $('<div class="invalid-feedback"></div>');
                            $field.after($errorMsg);
                        }

                        $errorMsg.text($field.data('error-message') || 'Este campo es obligatorio').show();
                    } else {
                        // Validación personalizada por campo
                        if ($field.data('validate') && typeof window[$field.data('validate')] === 'function') {
                            const validationResult = window[$field.data('validate')]($field);
                            if (validationResult !== true) {
                                $field.addClass('is-invalid');
                                let $errorMsg = $field.next('.invalid-feedback');
                                if ($errorMsg.length === 0) {
                                    $errorMsg = $('<div class="invalid-feedback"></div>');
                                    $field.after($errorMsg);
                                }
                                $errorMsg.text(validationResult).show();
                                return;
                            }
                        }

                        $field.removeClass('is-invalid').addClass('is-valid');
                        $field.next('.invalid-feedback').hide();
                    }
                });
            }

            // Reiniciar formulario
            if (options.resetButtonSelector) {
                $(options.resetButtonSelector).on('click', function (e) {
                    e.preventDefault();
                    $form[0].reset();
                    $form.find('.is-invalid, .is-valid').removeClass('is-invalid is-valid');
                    $form.find('.invalid-feedback').hide();

                    if (options.afterReset && typeof options.afterReset === 'function') {
                        options.afterReset($form);
                    }
                });
            }
        },

        // COMPONENTES DE TABLAS

        // Configurar botón de eliminar fila
        initDeleteButton: function () {
            $(document).on('click', '.eliminar-producto, .remove-product', function () {
                const $row = $(this).closest('tr');
                const $table = $row.closest('table');

                $row.remove();

                // Actualizar totales si aplica
                const tableId = $table.attr('id');
                if (tableId) {
                    App.forms.reindexRows(tableId);
                    App.forms.updateTotals(tableId);
                }
            });
        },

        // Configurar cambio de cantidad
        initQuantityChange: function () {
            $(document).on('change', '.cantidad', function () {
                const $row = $(this).closest('tr');
                const $table = $row.closest('table');
                const cantidad = parseInt($(this).val());
                const precio = parseFloat($row.find('input[name$=".PrecioUnitario"]').val());
                const subtotal = cantidad * precio;

                // Formatear con currency si está disponible
                const formattedSubtotal = App.format?.currency ?
                    App.format.currency(subtotal) :
                    new Intl.NumberFormat('es-AR', {
                        style: 'currency',
                        currency: 'ARS'
                    }).format(subtotal);

                $row.find('.subtotal').text(formattedSubtotal);
                $row.find('input[name$=".PrecioTotal"]').val(subtotal);

                // Actualizar totales de tabla
                const tableId = $table.attr('id');
                if (tableId) {
                    App.forms.updateTotals(tableId);
                }
            });
        },

        // Reindexar filas para envío del formulario
        reindexRows: function (tableId) {
            $(`#${tableId} tbody tr`).each(function (index) {
                $(this).attr('data-index', index);

                $(this).find('input').each(function () {
                    const name = $(this).attr('name');
                    if (name && name.includes('[')) {
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

            $(`#${tableId} tbody tr`).each(function () {
                const cantidad = parseInt($(this).find('.cantidad').val()) || 0;
                const precio = parseFloat($(this).find('input[name$=".PrecioUnitario"]').val()) || 0;
                const subtotal = cantidad * precio;

                totalProducts += cantidad;
                totalAmount += subtotal;

                // Actualizar campo oculto para envío del formulario
                $(this).find('input[name$=".PrecioTotal"]').val(subtotal);
            });

            // Actualizar totales en la UI
            const totalProductsId = options.totalProductsId || 'totalProductos';
            const totalAmountId = options.totalAmountId || 'totalVenta';

            $(`#${totalProductsId}`).text(totalProducts);

            // Usar formatter si está disponible
            const formattedAmount = App.format?.currency ?
                App.format.currency(totalAmount) :
                new Intl.NumberFormat('es-AR', {
                    style: 'currency',
                    currency: 'ARS'
                }).format(totalAmount);

            $(`#${totalAmountId}`).text(formattedAmount);

            // Establecer total oculto si se proporciona
            if (options.hiddenTotalInput) {
                $(options.hiddenTotalInput).val(totalAmount);
            }

            return {
                products: totalProducts,
                amount: totalAmount
            };
        },

        // Toggle seleccionar todo
        initSelectAll: function (checkAllSelector, itemSelector) {
            $(document).on('change', checkAllSelector, function () {
                const isChecked = $(this).prop('checked');
                $(itemSelector).prop('checked', isChecked);

                // Resaltar filas si es necesario
                if ($(itemSelector).closest('tr').length) {
                    $(itemSelector).closest('tr').toggleClass('table-active', isChecked);
                }
            });

            // Cambio de checkbox individual
            $(document).on('change', itemSelector, function () {
                const allChecked = $(itemSelector).length === $(itemSelector + ':checked').length;
                $(checkAllSelector).prop('checked', allChecked);

                // Resaltar fila
                $(this).closest('tr').toggleClass('table-active', this.checked);
            });
        },

        // Configurar tabla con ordenamiento y filtrado
        setupDataTable: function (tableSelector, options) {
            options = options || {};

            const $table = $(tableSelector);
            if (!$table.length) return;

            // Opciones por defecto para DataTable
            const defaultOptions = {
                language: {
                    search: "Buscar:",
                    lengthMenu: "Mostrar _MENU_ registros por página",
                    zeroRecords: "No se encontraron registros",
                    info: "Mostrando página _PAGE_ de _PAGES_",
                    infoEmpty: "No hay registros disponibles",
                    infoFiltered: "(filtrado de _MAX_ registros totales)",
                    paginate: {
                        first: "Primero",
                        last: "Último",
                        next: "Siguiente",
                        previous: "Anterior"
                    }
                },
                responsive: true,
                pageLength: options.pageLength || 10,
                lengthMenu: options.lengthMenu || [10, 25, 50, 100]
            };

            // Combinar opciones por defecto con las proporcionadas
            const dataTableOptions = $.extend({}, defaultOptions, options.dataTableOptions || {});

            // Inicializar DataTable si está disponible
            if ($.fn.DataTable) {
                return $table.DataTable(dataTableOptions);
            }

            // Si DataTable no está disponible, implementar un filtrado básico
            if (options.filterInputSelector) {
                $(options.filterInputSelector).on('input', function () {
                    const searchTerm = $(this).val().toLowerCase();

                    $table.find('tbody tr').each(function () {
                        const rowText = $(this).text().toLowerCase();
                        $(this).toggle(rowText.indexOf(searchTerm) > -1);
                    });
                });
            }

            // Permitir ordenamiento básico si se solicita
            if (options.enableBasicSorting) {
                $table.find('thead th[data-sort]').css('cursor', 'pointer').on('click', function () {
                    const column = $(this).index();
                    const sortDir = $(this).attr('data-sort-dir') === 'asc' ? 'desc' : 'asc';

                    // Actualizar dirección de ordenamiento
                    $table.find('thead th').removeAttr('data-sort-dir');
                    $(this).attr('data-sort-dir', sortDir);

                    // Ordenar filas
                    const rows = $table.find('tbody tr').toArray().sort(function (a, b) {
                        const aValue = $(a).find('td').eq(column).text();
                        const bValue = $(b).find('td').eq(column).text();

                        // Determinar tipo de datos
                        if (!isNaN(parseFloat(aValue)) && !isNaN(parseFloat(bValue))) {
                            // Comparación numérica
                            return sortDir === 'asc' ?
                                parseFloat(aValue) - parseFloat(bValue) :
                                parseFloat(bValue) - parseFloat(aValue);
                        } else {
                            // Comparación alfabética
                            return sortDir === 'asc' ?
                                aValue.localeCompare(bValue) :
                                bValue.localeCompare(aValue);
                        }
                    });

                    // Reemplazar filas en la tabla
                    $table.find('tbody').empty().append(rows);
                });
            }

            return null; // Retornar null si no se pudo inicializar DataTable
        },

        // Inicializar tabla editable
        initEditableTable: function (tableSelector, options) {
            options = options || {};

            const $table = $(tableSelector);
            if (!$table.length) return;

            // Botón para agregar fila
            if (options.addRowButtonSelector) {
                $(options.addRowButtonSelector).on('click', function () {
                    const rowTemplate = options.rowTemplate || '';
                    const $tbody = $table.find('tbody');
                    const rowIndex = $tbody.find('tr').length;

                    // Reemplazar índice en la plantilla
                    const newRow = rowTemplate.replace(/\{index\}/g, rowIndex);
                    $tbody.append(newRow);

                    // Callback después de agregar fila
                    if (options.afterAddRow && typeof options.afterAddRow === 'function') {
                        options.afterAddRow($tbody.find('tr').last(), rowIndex);
                    }
                });
            }

            // Botón para eliminar fila
            $table.on('click', options.deleteRowButtonSelector || '.delete-row', function () {
                const $row = $(this).closest('tr');

                // Confirmar eliminación si es necesario
                if (options.confirmDelete) {
                    if (!confirm(options.confirmDeleteMessage || '¿Está seguro que desea eliminar esta fila?')) {
                        return;
                    }
                }

                // Callback antes de eliminar
                if (options.beforeDeleteRow && typeof options.beforeDeleteRow === 'function') {
                    const result = options.beforeDeleteRow($row);
                    if (result === false) return;
                }

                // Eliminar fila
                $row.remove();

                // Reindexar filas si es necesario
                if (options.reindexAfterDelete !== false) {
                    App.forms.reindexRows($table.attr('id'));
                }

                // Callback después de eliminar
                if (options.afterDeleteRow && typeof options.afterDeleteRow === 'function') {
                    options.afterDeleteRow();
                }
            });

            // Inicializar campos editables
            if (options.editableSelector) {
                $table.on('click', options.editableSelector, function () {
                    const $cell = $(this);
                    if ($cell.find('input, select, textarea').length) return;

                    const currentValue = $cell.text().trim();
                    const fieldType = $cell.data('type') || 'text';
                    const fieldName = $cell.data('name') || '';

                    let inputHtml = '';

                    // Crear input según tipo
                    switch (fieldType) {
                        case 'select':
                            const optionsData = $cell.data('options') || '';
                            const optionsArray = optionsData.split('|');

                            inputHtml = `<select class="form-control form-control-sm edit-field" name="${fieldName}">`;
                            optionsArray.forEach(opt => {
                                const [value, text] = opt.split(':');
                                const selected = value === currentValue ? 'selected' : '';
                                inputHtml += `<option value="${value}" ${selected}>${text || value}</option>`;
                            });
                            inputHtml += '</select>';
                            break;

                        case 'number':
                            inputHtml = `<input type="number" class="form-control form-control-sm edit-field" name="${fieldName}" value="${currentValue}">`;
                            break;

                        default:
                            inputHtml = `<input type="text" class="form-control form-control-sm edit-field" name="${fieldName}" value="${currentValue}">`;
                    }

                    // Reemplazar contenido con input
                    $cell.html(inputHtml);
                    $cell.find('.edit-field').focus().select();

                    // Guardar al perder foco
                    $cell.find('.edit-field').on('blur', function () {
                        const newValue = $(this).val();

                        // Callback antes de guardar
                        if (options.beforeSaveCell && typeof options.beforeSaveCell === 'function') {
                            const result = options.beforeSaveCell($cell, newValue, currentValue);
                            if (result === false) {
                                $cell.text(currentValue);
                                return;
                            }
                        }

                        // Actualizar celda
                        $cell.text(newValue);

                        // Callback después de guardar
                        if (options.afterSaveCell && typeof options.afterSaveCell === 'function') {
                            options.afterSaveCell($cell, newValue, currentValue);
                        }
                    });

                    // Guardar al presionar Enter
                    $cell.find('.edit-field').on('keypress', function (e) {
                        if (e.which === 13) {
                            $(this).blur();
                            return false;
                        }
                    });
                });
            }
        }
    };

    // Alias para compatibilidad con código existente
    App.tables = {
        init: function () {
            App.forms.initDeleteButton();
            App.forms.initQuantityChange();
        },
        initDeleteButton: App.forms.initDeleteButton,
        initQuantityChange: App.forms.initQuantityChange,
        reindexRows: App.forms.reindexRows,
        updateTotals: App.forms.updateTotals,
        initSelectAll: App.forms.initSelectAll
    };

})(window, jQuery);