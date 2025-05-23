// wwwroot/js/utils/client-search.js

import { ajaxPost } from './app.js';
import { notify } from './app.js';

/**
 * Configura búsqueda de cliente por DNI.
 * @param {string} btnSelector - Selector del botón de búsqueda.
 * @param {string} inputSelector - Selector del input de DNI.
 * @param {Object} handlers - { onSuccess, onError }
 */
export function setupClienteSearch(btnSelector, inputSelector, { onSuccess, onError }) {
    const btn = document.querySelector(btnSelector);
    const input = document.querySelector(inputSelector);
    if (!btn || !input) return;

    // Búsqueda al hacer clic
    btn.addEventListener('click', async () => {
        const dni = input.value.trim();
        if (!dni) {
            notify.warning('Ingrese un DNI válido');
            return;
        }
        try {
            const cliente = await ajaxPost('/Clientes/BuscarPorDni', { dni });
            onSuccess(cliente);
        } catch (e) {
            console.error('Error buscando cliente:', e);
            onError(e);
        }
    });

    // Búsqueda con Enter
    input.addEventListener('keypress', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            btn.click();
        }
    });
}
