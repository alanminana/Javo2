// devolucion-form.js - Módulo para devoluciones y garantías
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.devolucionForm = {
        init: function () {
            this.setupEventHandlers();
        },

        setupEventHandlers: function () {
            // Búsqueda de venta
            $("#btnBuscarVenta").click(function () {
                var numeroFactura = $("#buscarVenta").val();
                if (!numeroFactura) {
                    App.devolucionForm.showError('Ingrese un número de factura válido');
                    return;
                }

                $("#errorBusqueda").addClass("d-none");

                $.ajax({
                    url: '/DevolucionGarantia/BuscarVenta',
                    type: 'POST',
                    data: { numeroFactura: numeroFactura },
                    success: function (result) {
                        if (result.success) {
                            $("#formFields").removeClass("d-none");
                            $("#paso1").hide();

                            App.devolucionForm.renderProductTable(result.items);
                        } else {
                            App.devolucionForm.showError(result.message);
                        }
                    },
                    error: function () {
                        App.devolucionForm.showError('Error al buscar la venta');
                    }
                });
            });

            // Seleccionar todos
            $("#selectAllProducts").change(function () {
                $(".product-check").prop('checked', $(this).prop('checked'));
            });

            // Cambio de TipoCaso
            $("#TipoCaso").change(function () {
                if ($(this).val() == '1') { // Assuming 1 is the ID for Cambio
                    App.devolucionForm.setupCambioProductos();
                    $("#seccionCambio").removeClass("d-none");
                } else {
                    $("#seccionCambio").addClass("d-none");
                }
            });

            // Búsqueda por enter
            $("#buscarVenta").keypress(function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $("#btnBuscarVenta").click();
                }
            });
        },

        // Renderizar tabla de productos de venta
        renderProductTable: function (items) {
            var tbody = $("#tablaProductos tbody").empty();

            items.forEach(function (item, i) {
                tbody.append(
                    `<tr data-id="${item.productoID}">
                      <td>
                        <input type="checkbox" class="product-check" name="Items[${i}].Seleccionado" />
                        <input type="hidden" name="Items[${i}].ProductoID" value="${item.productoID}" />
                        <input type="hidden" name="Items[${i}].NombreProducto" value="${item.nombreProducto}" />
                        <input type="hidden" name="Items[${i}].PrecioUnitario" value="${item.precioUnitario}" />
                      </td>
                      <td>${item.nombreProducto}</td>
                      <td><input type="number" class="form-control" name="Items[${i}].Cantidad" value="${item.cantidad}" min="1"/></td>
                      <td>${this.formatCurrency(item.precioUnitario)}</td>
                      <td>${this.formatCurrency(item.subtotal)}</td>
                      <td>
                        <select class="form-select" name="Items[${i}].EstadoProducto">
                          <option>Funcional</option>
                          <option>Defectuoso</option>
                          <option>Dañado</option>
                          <option>Incompleto</option>
                          <option>Sin usar</option>
                        </select>
                      </td>
                      <td><input type="checkbox" name="Items[${i}].ProductoDanado" /></td>
                    </tr>`);
            });
        },

        // Configurar tabla de cambios de productos
        setupCambioProductos: function () {
            var tabla = $("#tablaCambios tbody").empty();

            $(".product-check:checked").each(function (i) {
                var row = $(this).closest('tr'),
                    id = row.data('id'),
                    nm = row.find('td:eq(1)').text(),
                    qt = row.find('input[name$=".Cantidad"]').val();

                tabla.append(
                    `<tr>
                      <td>${nm}<input type="hidden" name="CambiosProducto[${i}].ProductoOriginalID" value="${id}" /></td>
                      <td>
                        <input type="text" class="form-control producto-nuevo-buscar" placeholder="Buscar producto..." />
                        <input type="hidden" name="CambiosProducto[${i}].ProductoNuevoID" class="producto-nuevo-id" />
                        <input type="hidden" name="CambiosProducto[${i}].NombreProductoNuevo" class="producto-nuevo-nombre" />
                      </td>
                      <td><input type="number" class="form-control" name="CambiosProducto[${i}].Cantidad" value="${qt}" min="1"/></td>
                      <td><input type="number" class="form-control" name="CambiosProducto[${i}].DiferenciaPrecio" step="0.01"/></td>
                      <td><button type="button" class="btn btn-sm btn-outline-danger eliminar-cambio">✕</button></td>
                    </tr>`);
            });

            $(".producto-nuevo-buscar").autocomplete({
                source: function (r, s) {
                    $.getJSON('/DevolucionGarantia/BuscarProductos', { term: r.term }, s);
                },
                minLength: 2,
                select: function (e, ui) {
                    $(this).val(ui.item.label);
                    $(this).siblings('.producto-nuevo-id').val(ui.item.id);
                    $(this).siblings('.producto-nuevo-nombre').val(ui.item.label);
                    return false;
                }
            });

            // Manejador para eliminar cambio
            $(document).on('click', '.eliminar-cambio', function () {
                $(this).closest('tr').remove();
            });
        },

        // Mostrar error
        showError: function (message) {
            $("#errorBusqueda").text(message).removeClass('d-none');
        },

        // Formatear moneda
        formatCurrency: function (value) {
            return App.format.currency(value);
        }
    };

    // Módulo para detalles y listados
    App.devolucionUtils = {
        init: function () {
            // Ocultar alertas después de 5 segundos
            setTimeout(function () {
                $('.alert').alert('close');
            }, 5000);
        }
    };

})(window, jQuery);