// utils/permission-handler.js
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.permissions = {
        // Caché de permisos
        cache: {},

        // Permisos cargados del servidor
        userPermissions: [],

        // Inicializar sistema de permisos
        init: function () {
            this.loadPermissions();
            this.applyPermissions();
        },

        // Cargar permisos del DOM
        loadPermissions: function () {
            const permData = document.getElementById('userPermissions');
            if (permData && permData.dataset.permissions) {
                try {
                    this.userPermissions = JSON.parse(permData.dataset.permissions);
                } catch (e) {
                    console.error('Error al parsear permisos:', e);
                    this.userPermissions = [];
                }
            }
        },

        // Aplicar permisos a todos los elementos
        applyPermissions: function () {
            // Ocultar elementos sin permiso
            document.querySelectorAll("[data-require-permission]").forEach(element => {
                const permission = element.getAttribute("data-require-permission");
                if (!this.hasPermission(permission)) {
                    element.style.display = "none";
                }
            });

            // Deshabilitar botones sin permiso
            document.querySelectorAll("button[data-require-permission], a[data-require-permission]").forEach(element => {
                const permission = element.getAttribute("data-require-permission");
                if (!this.hasPermission(permission)) {
                    element.disabled = true;
                    element.classList.add("disabled");

                    // Prevenir click en enlaces
                    if (element.tagName === 'A') {
                        element.addEventListener('click', (e) => {
                            e.preventDefault();
                            return false;
                        });
                    }
                }
            });

            // Eliminar eventos de elementos sin permiso
            document.querySelectorAll("[data-require-permission][data-remove-events]").forEach(element => {
                const permission = element.getAttribute("data-require-permission");
                if (!this.hasPermission(permission)) {
                    const clone = element.cloneNode(true);
                    element.parentNode.replaceChild(clone, element);
                }
            });
        },

        // Verificar si el usuario tiene un permiso
        hasPermission: function (permissionCode) {
            if (!permissionCode) return true;

            // Verificar caché primero
            if (this.cache[permissionCode] !== undefined) {
                return this.cache[permissionCode];
            }

            // Verificar permisos cargados
            const result = this.userPermissions.includes(permissionCode);
            this.cache[permissionCode] = result;

            return result;
        },

        // Comprobar permiso y ejecutar acción
        withPermission: function (permissionCode, action, fallback) {
            if (this.hasPermission(permissionCode)) {
                if (typeof action === 'function') {
                    return action();
                }
                return true;
            } else {
                if (typeof fallback === 'function') {
                    return fallback();
                }
                return false;
            }
        }
    };

})(window, jQuery);