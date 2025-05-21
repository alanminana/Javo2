// permissions.js - Módulo para gestión de permisos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.permissions = {
        cache: {},

        init: function () {
            this.setupPermissionElements();
        },

        setupPermissionElements: function () {
            document.querySelectorAll("[data-require-permission]").forEach(function (element) {
                const permission = element.getAttribute("data-require-permission");
                if (!App.permissions.hasPermission(permission)) {
                    element.style.display = "none";
                }
            });

            document.querySelectorAll("button[data-require-permission]").forEach(function (button) {
                const permission = button.getAttribute("data-require-permission");
                if (!App.permissions.hasPermission(permission)) {
                    button.disabled = true;
                    button.classList.add("disabled");
                }
            });

            document.querySelectorAll("[data-require-permission]").forEach(function (element) {
                const permission = element.getAttribute("data-require-permission");
                if (!App.permissions.hasPermission(permission)) {
                    const clone = element.cloneNode(true);
                    element.parentNode.replaceChild(clone, element);
                }
            });
        },

        hasPermission: function (permissionCode) {
            if (this.cache[permissionCode] !== undefined) {
                return this.cache[permissionCode];
            }

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

            const hasDirectPermission = document.querySelector(`[data-permission-code="${permissionCode}"]`) !== null;
            this.cache[permissionCode] = hasDirectPermission;
            return hasDirectPermission;
        }
    };

})(window, jQuery);