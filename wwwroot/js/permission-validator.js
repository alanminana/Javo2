// wwwroot/js/permission-validator.js
// Script para validar permisos en peticiones AJAX y elementos DOM

class PermissionValidator {
    constructor() {
        this.permissions = this._getPermissions();
        this._setupAjaxInterceptor();
        this._processRequirePermissionElements();
    }

    _getPermissions() {
        const permissionsAttr = document.body.getAttribute('data-permissions');
        if (!permissionsAttr) return [];

        try {
            return JSON.parse(permissionsAttr);
        } catch (e) {
            console.error('Error parsing permissions:', e);
            return [];
        }
    }

    hasPermission(permissionCode) {
        return this.permissions.includes(permissionCode);
    }

    _processRequirePermissionElements() {
        document.querySelectorAll('[require-permission]').forEach(element => {
            const permission = element.getAttribute('require-permission');
            if (!this.hasPermission(permission)) {
                if (element.tagName === 'BUTTON' || element.tagName === 'INPUT') {
                    element.disabled = true;
                    element.classList.add('disabled');
                    // Eliminar handlers para prevenir ejecución
                    const clone = element.cloneNode(true);
                    element.parentNode.replaceChild(clone, element);
                } else {
                    element.style.display = 'none';
                }
            }
        });
    }

    _setupAjaxInterceptor() {
        if (typeof jQuery !== 'undefined') {
            const self = this;

            $(document).ajaxSend(function (event, xhr, settings) {
                const urlParts = settings.url.split('/');
                if (urlParts.length >= 3) {
                    const controller = urlParts[1].toLowerCase();
                    const action = urlParts[2].toLowerCase();
                    let requiredPermission = null;

                    if (action.startsWith('edit') || action.startsWith('update')) {
                        requiredPermission = `${controller}.editar`;
                    } else if (action.startsWith('delet') || action.startsWith('remove')) {
                        requiredPermission = `${controller}.eliminar`;
                    } else if (action.startsWith('creat') || action.startsWith('add')) {
                        requiredPermission = `${controller}.crear`;
                    } else if (action.startsWith('detail') || action.startsWith('view') || action === 'index') {
                        requiredPermission = `${controller}.ver`;
                    }

                    if (requiredPermission && !self.hasPermission(requiredPermission)) {
                        xhr.abort();
                        console.error(`Permiso denegado: ${requiredPermission} es requerido para ${settings.url}`);

                        if (typeof showToast === 'function') {
                            showToast('error', 'Permiso denegado', 'No tienes permiso para realizar esta acción');
                        } else {
                            alert('No tienes permiso para realizar esta acción');
                        }
                    }
                }
            });
        }

        if (window.fetch) {
            const originalFetch = window.fetch;
            const self = this;

            window.fetch = function (url, options) {
                const urlParts = url.split('/');
                if (urlParts.length >= 3) {
                    const controller = urlParts[1].toLowerCase();
                    const action = urlParts[2].toLowerCase();
                    let requiredPermission = null;

                    if (action.startsWith('edit') || action.startsWith('update')) {
                        requiredPermission = `${controller}.editar`;
                    } else if (action.startsWith('delet') || action.startsWith('remove')) {
                        requiredPermission = `${controller}.eliminar`;
                    } else if (action.startsWith('creat') || action.startsWith('add')) {
                        requiredPermission = `${controller}.crear`;
                    } else if (action.startsWith('detail') || action.startsWith('view') || action === 'index') {
                        requiredPermission = `${controller}.ver`;
                    }

                    if (requiredPermission && !self.hasPermission(requiredPermission)) {
                        return Promise.reject(new Error(`Permiso denegado: ${requiredPermission} es requerido para ${url}`));
                    }
                }

                return originalFetch.apply(this, arguments);
            };
        }
    }
}

// Exponer función global para verificar permisos
window.hasPermission = function (permissionCode) {
    return window.permissionValidator ?
        window.permissionValidator.hasPermission(permissionCode) :
        false;
};

// Inicializar el validador cuando se carga el DOM
document.addEventListener('DOMContentLoaded', function () {
    window.permissionValidator = new PermissionValidator();
});