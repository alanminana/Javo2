// permissions.js - Módulo para manejo de permisos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.permissions = {
        cache: {},

        init: function () {
            this.setupPermissionElements();
        },

        // Configurar elementos basados en permisos
        setupPermissionElements: function () {
            // Ocultar elementos que requieren permisos
            document.querySelectorAll('[data-require-permission]').forEach(function (element) {
                const permission = element.getAttribute('data-require-permission');
                if (!App.permissions.hasPermission(permission)) {
                    element.style.display = 'none';
                }
            });

            // Deshabilitar botones que requieren permisos
            document.querySelectorAll('button[data-require-permission]').forEach(function (button) {
                const permission = button.getAttribute('data-require-permission');
                if (!App.permissions.hasPermission(permission)) {
                    button.disabled = true;
                    button.classList.add('disabled');
                }
            });

            // Quitar eventos de elementos sin permiso
            document.querySelectorAll('[data-require-permission]').forEach(function (element) {
                const permission = element.getAttribute('data-require-permission');
                if (!App.permissions.hasPermission(permission)) {
                    // Clonar y reemplazar para eliminar eventos
                    const clone = element.cloneNode(true);
                    element.parentNode.replaceChild(clone, element);
                }
            });
        },

        // Verificar si el usuario tiene un permiso
        hasPermission: function (permissionCode) {
            if (this.cache[permissionCode] !== undefined) {
                return this.cache[permissionCode];
            }

            // Intentar obtener del elemento data
            const permData = document.getElementById('userPermissions');
            if (permData && permData.dataset.permissions) {
                try {
                    const permissions = JSON.parse(permData.dataset.permissions);
                    const result = permissions.includes(permissionCode);
                    this.cache[permissionCode] = result;
                    return result;
                } catch (e) {
                    console.error('Error al parsear permisos:', e);
                }
            }

            // Fallback: buscar en elementos específicos
            const hasDirectPermission = document.querySelector(`[data-permission-code="${permissionCode}"]`) !== null;
            this.cache[permissionCode] = hasDirectPermission;
            return hasDirectPermission;
        }
    };

})(window, jQuery);