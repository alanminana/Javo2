// wwwroot/js/permission-validator.js
// Script para validar permisos en peticiones AJAX

// Clase para la validación de permisos
class PermissionValidator {
    constructor() {
        this.permissions = this._getPermissions();
        this._setupAjaxInterceptor();
    }

    // Obtener permisos del data-attribute
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

    // Verificar si el usuario tiene un permiso específico
    hasPermission(permissionCode) {
        return this.permissions.includes(permissionCode);
    }

    // Configurar interceptor para peticiones AJAX
    _setupAjaxInterceptor() {
        if (typeof jQuery !== 'undefined') {
            const self = this;

            // Interceptar solicitudes AJAX de jQuery
            $(document).ajaxSend(function (event, xhr, settings) {
                // Extraer controlador y acción de la URL
                const urlParts = settings.url.split('/');
                if (urlParts.length >= 3) {
                    const controller = urlParts[1].toLowerCase();
                    const action = urlParts[2].toLowerCase();

                    // Determinar el permiso requerido
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

                    // Verificar permiso
                    if (requiredPermission && !self.hasPermission(requiredPermission)) {
                        // Cancelar la solicitud
                        xhr.abort();
                        console.error(`Permiso denegado: ${requiredPermission} es requerido para ${settings.url}`);

                        // Mostrar mensaje
                        if (typeof showToast === 'function') {
                            showToast('error', 'Permiso denegado', 'No tienes permiso para realizar esta acción');
                        } else {
                            alert('No tienes permiso para realizar esta acción');
                        }
                    }
                }
            });
        }

        // Interceptar fetch si está disponible
        if (window.fetch) {
            const originalFetch = window.fetch;
            const self = this;

            window.fetch = function (url, options) {
                // Extraer controlador y acción de la URL
                const urlParts = url.split('/');
                if (urlParts.length >= 3) {
                    const controller = urlParts[1].toLowerCase();
                    const action = urlParts[2].toLowerCase();

                    // Determinar el permiso requerido
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

                    // Verificar permiso
                    if (requiredPermission && !self.hasPermission(requiredPermission)) {
                        // Rechazar la promesa con un error
                        return Promise.reject(new Error(`Permiso denegado: ${requiredPermission} es requerido para ${url}`));
                    }
                }

                // Continuar con la solicitud original
                return originalFetch.apply(this, arguments);
            };
        }
    }
}

// Inicializar el validador de permisos
document.addEventListener('DOMContentLoaded', function () {
    window.permissionValidator = new PermissionValidator();
});