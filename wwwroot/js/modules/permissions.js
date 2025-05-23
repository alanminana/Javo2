// wwwroot/js/components/permissions.js

/**
 * Componente para manejar visibilidad y habilitación de elementos según permisos de usuario.
 *
 * Reemplaza al antiguo permissions.js sin depender de jQuery ni de un objeto global.
 */
export const permissions = {
    // Cache interno de permisos { [code]: boolean }
    cache: {},

    /**
     * Inicializa la lógica de permisos: oculta/deshabilita elementos que requieran permisos.
     * Debe ejecutarse en DOMContentLoaded.
     */
    init() {
        // Carga inicial de permisos desde dataset si está presente
        this._loadInitialPermissions();

        // Procesa elementos con data-require-permission
        document.querySelectorAll('[data-require-permission]').forEach(el => {
            const code = el.getAttribute('data-require-permission');
            if (!this.hasPermission(code)) {
                // Ocultar
                el.style.display = 'none';
                // Deshabilitar
                el.disabled = true;
                // Reemplaza nodo para quitar listeners
                const clone = el.cloneNode(true);
                el.parentNode.replaceChild(clone, el);
            }
        });

        // Procesa elementos con data-permission-code (inclusión directa)
        document.querySelectorAll('[data-permission-code]').forEach(el => {
            const code = el.getAttribute('data-permission-code');
            if (!this.hasPermission(code)) {
                el.remove();
            }
        });
    },

    /**
     * Verifica si el usuario tiene el permiso indicado.
     * @param {string} code - Código de permiso a comprobar.
     * @returns {boolean}
     */
    hasPermission(code) {
        // Retorna si está cacheado
        if (this.cache[code] !== undefined) {
            return this.cache[code];
        }

        // Si se cargaron permisos iniciales
        if (this.initialPermissions) {
            const allowed = this.initialPermissions.includes(code);
            this.cache[code] = allowed;
            return allowed;
        }

        // Fallback: buscar elemento con data-permission-code
        const direct = !!document.querySelector(`[data-permission-code="${code}"]`);
        this.cache[code] = direct;
        return direct;
    },

    /**
     * Carga permisos iniciales desde un elemento con id "userPermissions" y data-permissions
     * Formato esperado: <div id="userPermissions" data-permissions='["perm1","perm2"]'></div>
     */
    _loadInitialPermissions() {
        const permEl = document.getElementById('userPermissions');
        if (!permEl) return;

        try {
            const perms = JSON.parse(permEl.dataset.permissions);
            if (Array.isArray(perms)) {
                this.initialPermissions = perms;
                // Poblamos cache inicialmente
                perms.forEach(code => { this.cache[code] = true; });
            }
        } catch (e) {
            console.error('Error parsing user permissions:', e);
        }
    }
};

// Auto-inicialización
document.addEventListener('DOMContentLoaded', () => permissions.init());
