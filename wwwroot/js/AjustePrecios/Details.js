$(function () {
    $('#btnRevertir').click(function (e) {
        if (!confirm('¿Está seguro que desea revertir este ajuste de precios? Esta acción restaurará los precios anteriores al ajuste.')) {
            e.preventDefault();
            return false;
        }
        return true;
    });
});