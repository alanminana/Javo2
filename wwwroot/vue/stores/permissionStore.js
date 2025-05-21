import { defineStore } from 'pinia';

export const usePermissionStore = defineStore('permissions', {
    state: () => ({
        userPermissions: [],
        permissionsLoaded: false
    }),

    actions: {
        loadPermissions() {
            const permData = document.body.dataset.permissions;
            if (permData) {
                try {
                    this.userPermissions = JSON.parse(permData);
                    this.permissionsLoaded = true;
                } catch (e) {
                    console.error('Error al parsear permisos:', e);
                    this.userPermissions = [];
                }
            }
        },

        hasPermission(permissionCode) {
            if (!permissionCode) return true;
            return this.userPermissions.includes(permissionCode);
        }
    }
});