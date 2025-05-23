// utils/confirm-action.js
import { ajaxPost } from './app.js';
import { notify } from './app.js';

/**
 * Muestra confirmación y ejecuta callback si acepta.
 */
export async function confirmAction(message, callback) {
    if (confirm(message)) {
        await callback();
    }
}

/**
 * Confirmación con POST automático.
 */
export async function confirmPost(message, url, data, { onSuccess, onError } = {}) {
    if (confirm(message)) {
        try {
            const result = await ajaxPost(url, data);
            if (onSuccess) onSuccess(result);
            return result;
        } catch (e) {
            console.error('Error en confirmPost:', e);
            if (onError) onError(e);
            throw e;
        }
    }
}