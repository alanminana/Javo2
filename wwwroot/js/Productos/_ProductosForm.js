
        $(function() {
            // Cargar subrubros cuando cambia el rubro
            $('#rubroDropdown').change(function() {
                var rubroId = $(this).val();
                if (rubroId) {
                    $.getJSON('@Url.Action("GetSubRubros")', { rubroId: rubroId }, function(data) {
                        var subRubroSelect = $('#subRubroDropdown');
                        subRubroSelect.empty();
                        subRubroSelect.append($('<option></option>').val('').text('-- Seleccione SubRubro --'));
                        $.each(data, function(index, item) {
                            subRubroSelect.append($('<option></option>').val(item.value).text(item.text));
                        });
                    });
                } else {
                    $('#subRubroDropdown').empty().append($('<option></option>').val('').text('-- Seleccione SubRubro --'));
                }
            });

            // Al cambiar el precio de costo, simular cálculo para mostrar (no afecta el valor real)
            $('#pCosto').on('input', function() {
                const pCosto = parseFloat($(this).val()) || 0;
                const pContadoPorc = @(await GetConfigValue("Productos", "PorcentajeGananciaPContado", 50));
                const pListaPorc = @(await GetConfigValue("Productos", "PorcentajeGananciaPLista", 84));

                const pContado = pCosto * (1 + pContadoPorc / 100);
                const pLista = pCosto * (1 + pListaPorc / 100);

                $('#PContado').val(pContado.toFixed(2));
                $('#PLista').val(pLista.toFixed(2));
            });
        });


@functions {
    private async Task<decimal> GetConfigValue(string modulo, string clave, decimal valorPorDefecto)
    {
        var configuracionService = Context.RequestServices.GetService<Javo2.IServices.IConfiguracionService>();
        if (configuracionService != null)
        {
            return await configuracionService.GetValorAsync(modulo, clave, valorPorDefecto);
        }
        return valorPorDefecto;
    }
}