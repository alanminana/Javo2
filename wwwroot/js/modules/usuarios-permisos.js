// wwwroot/js/modules/usuarios-permisos.js
import { setupStateToggle } from '../utils/state-toggle.js';

const userPerms = {
    init() {
        // Botones con .toggle-user-state y data-id, data-active
        setupStateToggle(
            '.toggle-user-state',
            '#confirmStateModal',
            '#confirmStateBtn',
            '/Usuarios',
            'usuario'
        );
    }
};

document.addEventListener('DOMContentLoaded', () => userPerms.init());
export default userPerms;
