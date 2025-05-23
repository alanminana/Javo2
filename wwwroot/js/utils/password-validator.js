
/**
 * Valida fortaleza de contraseña.
 * Requiere 8+ caracteres, mayúscula, minúscula, dígito y carácter especial.
 * @param {string} pwd
 * @returns {boolean}
 */
export function validatePasswordStrength(pwd) {
    const rules = [/^[\s\S]{8,}$/, /[A-Z]/, /[a-z]/, /[0-9]/, /[^A-Za-z0-9]/];
    return rules.every(rx => rx.test(pwd));
}


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
