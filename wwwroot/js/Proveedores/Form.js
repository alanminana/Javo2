
        < script defer >
            (function ($) {
                $(function () {
                    var assigned = [];
                    $('#assignedProducts li').each(function () {
                        assigned.push($(this).data('id'));
                    });
                    $('#productSearch').autocomplete({
                        source: function (req, resp) {
                            $.getJSON('@Url.Action("SearchProducts", "Proveedores")', { term: req.term })
                                .done(resp).fail(function () { alert('Error'); });
                        },
                        select: function (e, ui) {
                            e.preventDefault();
                            var id = ui.item.value, name = ui.item.label;
                            if (!assigned.includes(id)) {
                                assigned.push(id);
                                $('#assignedProducts').append(
                                    `<li class="list-group-item d-flex justify-content-between align-items-center" data-id="${id}">` +
                                    `${name} <button type="button" class="btn btn-link btn-sm remove-product">Eliminar</button>` +
                                    `<input type="hidden" name="ProductosAsignados" value="${id}" />` +
                                    `</li>`);
                            }
                            $(this).val(''); return false;
                        }
                    });
                    $('#assignedProducts').on('click', '.remove-product', function () {
                        var li = $(this).closest('li'), id = li.data('id');
                        assigned = assigned.filter(x => x != id); li.remove();
                    });
                });
            })(jQuery);
