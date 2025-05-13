
            $(function(){
              // Búsqueda de venta
              $("#btnBuscarVenta").click(function(){
                var numeroFactura = $("#buscarVenta").val();
                if(!numeroFactura){ showError('Ingrese un número de factura válido'); return; }
                $("#errorBusqueda").addClass("d-none");
                $.ajax({
                  url: '@Url.Action("BuscarVenta")',
                  type: 'POST',
                  data: { numeroFactura: numeroFactura },
                  success: function(result){
                    if(result.success){
                      $("#formFields").removeClass("d-none");
                      $("#paso1").hide();
                      var tbody = $("#tablaProductos tbody").empty();
                      result.items.forEach(function(item,i){
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
          <td>${formatCurrency(item.precioUnitario)}</td>
          <td>${formatCurrency(item.subtotal)}</td>
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
                    } else showError(result.message);
                  },
                  error: function(){ showError('Error al buscar la venta'); }
                });
              });

              // Seleccionar todos
              $("#selectAllProducts").change(function(){
                $(".product-check").prop('checked', $(this).prop('checked'));
              });

              // Cambio de TipoCaso
              $("#TipoCaso").change(function(){
                if($(this).val() == '@((int)Javo2.Models.TipoCaso.Cambio)'){
                  setupCambioProductos();
                  $("#seccionCambio").removeClass("d-none");
                } else {
                  $("#seccionCambio").addClass("d-none");
                }
              });

              function setupCambioProductos(){
                var tabla = $("#tablaCambios tbody").empty();
                $(".product-check:checked").each(function(i){
                  var row = $(this).closest('tr'),
                      id  = row.data('id'),
                      nm  = row.find('td:eq(1)').text(),
                      qt  = row.find('input[name$=".Cantidad"]').val();
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
                  source: function(r,s){ $.getJSON('@Url.Action("BuscarProductos")',{ term:r.term },s); },
                  minLength: 2,
                  select: function(e,ui){
                    $(this).val(ui.item.label);
                    $(this).siblings('.producto-nuevo-id').val(ui.item.id);
                    $(this).siblings('.producto-nuevo-nombre').val(ui.item.label);
                    return false;
                  }
                });
              }

              $(document).on('click','.eliminar-cambio',function(){
                $(this).closest('tr').remove();
              });

              function showError(m){ $("#errorBusqueda").text(m).removeClass('d-none'); }
              function formatCurrency(v){ return new Intl.NumberFormat('es-AR',{style:'currency',currency:'ARS'}).format(v); }
            });
