
    document.getElementById('limpiarFiltro').addEventListener('click', function() {
        document.getElementById('Termino').value = '';
        document.getElementById('Activo').value = '';
        document.getElementById('RolID').value = '';
        document.getElementById('usuariosFilterForm').submit();
    });
