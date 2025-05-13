// productos-controller.js - Controlador centralizado para operaciones de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.productosController = {
        init: function () {
            this.setupEventListeners();
            this.initPriceCalculators();
        },

        // Configurar listeners de eventos
        setupEventListeners: function () {
            // Búsqueda de productos
            $(document).on('click', '.buscar-producto-btn', function () {
                const codigoInput = $(this).closest('.input-group').find('input').first();
                const codigo = codigoInput.val();

                App.products.searchByCode('/Productos/BuscarProducto', codigo, {
                    nameField: '#productoNombre',
                    priceField: '#productoPrecio',
                    quantityField: '#productoCantidad',
                    modalErrorId: '#productoNoEncontradoModal',
                    onSuccess: function (producto) {
                        // Almacenar producto actual en data del formulario
                        const form = codigoInput.closest('form');
                        form.data('productoActual', producto);
                    }
                });
            });

            // Agregar producto a tabla
            $(document).on('click', '.agregar-producto-btn', function () {
                const form = $(this).closest('form');
                const producto = form.data('productoActual');
                const cantidad = $('#productoCantidad').val();

                if (App.products.addToTable('productosTable', producto, cantidad, {
                    resetFields: true,
                    codeField: '#productoCodigo',
                    nameField: '#productoNombre',
                    priceField: '#productoPrecio'
                })) {
                    // Limpiar producto actual
                    form.data('productoActual', null);
                }
            });

            // Buscar al presionar Enter
            $(document).on('keypress', '.producto-codigo-input', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $(this).closest('.input-group').find('.buscar-producto-btn').click();
                }
            });
        },

        // Inicializar calculadoras de precios
        initPriceCalculators: function () {
            $(document).on('input', '#pCosto', function () {
                const pCosto = parseFloat($(this).val()) || 0;
                const pContadoPorc = parseFloat($('#porcentajeGananciaPContado').val()) || 50;
                const pListaPorc = parseFloat($('#porcentajeGananciaPLista').val()) || 84;

                const pContado = pCosto * (1 + pContadoPorc / 100);
                const pLista = pCosto * (1 + pListaPorc / 100);

                // Actualizar campos visibles
                $('#pContadoDisplay').val(pContado.toFixed(2));
                $('#pListaDisplay').val(pLista.toFixed(2));

                // Actualizar campos ocultos
                $('#PContado').val(pContado.toFixed(2));
                $('#PLista').val(pLista.toFixed(2));
            });
        },

        // Cargar SubRubros al seleccionar Rubro
        loadSubRubros: function (rubroId, subRubroDropdownId, currentSubRubroId, url) {
            if (!rubroId) {
                $(`#${subRubroDropdownId}`).empty().append('<option value="">-- Seleccione SubRubro --</option>');
                return;
            }

            $.getJSON(url || '/Catalogo/GetSubRubros', { rubroId: rubroId }, function (data) {
                const subRubroSelect = $(`#${subRubroDropdownId}`);
                subRubroSelect.empty();
                subRubroSelect.append('<option value="">-- Seleccione SubRubro --</option>');

                $.each(data, function (index, item) {
                    const option = $('<option></option>').val(item.value).text(item.text);
                    if (currentSubRubroId && item.value == currentSubRubroId) {
                        option.attr('selected', 'selected');
                    }
                    subRubroSelect.append(option);
                });
            });
        },

        // Ajuste de precios por porcentaje
        calculateAdjustedPrices: function (productos, porcentaje, esAumento) {
            const factor = esAumento ? (1 + porcentaje / 100) : (1 - porcentaje / 100);

            return productos.map(function (p) {
                const costoNuevo = p.costo * factor;
                const contadoNuevo = p.contado * factor;
                const listaNuevo = p.lista * factor;

                return {
                    ...p,
                    costoNuevo: costoNuevo,
                    contadoNuevo: contadoNuevo,
                    listaNuevo: listaNuevo
                };
            });
        }
    };

})(window, jQuery);