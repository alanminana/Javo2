// wwwroot/js/site.js (continuación)
document.addEventListener('DOMContentLoaded', function () {
    // Ocultar elementos que requieren permisos
    document.querySelectorAll('[data-require-permission]').forEach(function (element) {
        const permission = element.getAttribute('data-require-permission');
        if (!hasPermission(permission)) {
            element.style.display = 'none';
        }
    });

    // Deshabilitar botones que requieren permisos
    document.querySelectorAll('button[data-require-permission]').forEach(function (button) {
        const permission = button.getAttribute('data-require-permission');
        if (!hasPermission(permission)) {
            button.disabled = true;
            button.classList.add('disabled');
        }
    });

    // Quitar eventos de elementos sin permiso
    document.querySelectorAll('[data-require-permission]').forEach(function (element) {
        const permission = element.getAttribute('data-require-permission');
        if (!hasPermission(permission)) {
            // Clonar y reemplazar para eliminar eventos
            const clone = element.cloneNode(true);
            element.parentNode.replaceChild(clone, element);
        }
    });
});