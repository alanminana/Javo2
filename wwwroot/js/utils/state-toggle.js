
// wwwroot/js/utils/state-toggle.js
import { confirmPost } from './confirm-action.js';
import { notify } from './app.js';

/**
 * Configura toggles de estado (activar/desactivar) con confirmación.
 * @param {string} toggleSelector - Selector de botones toggles.
 * @param {string} modalSelector - Selector del modal de confirmación.
 * @param {string} confirmBtnSelector - Selector del botón de confirm en modal.
 * @param {string} urlBase - URL base para llamadas ('/Usuarios').
 * @param {string} entityName - Nombre de la entidad (p.ej. 'usuario').
 */
export function setupStateToggle(toggleSelector, modalSelector, confirmBtnSelector, urlBase, entityName) {
    document.querySelectorAll(toggleSelector).forEach(el => {
        el.addEventListener('click', () => {
            const id = el.dataset.id;
            const isActive = el.dataset.active === 'true';
            const modal = new bootstrap.Modal(document.querySelector(modalSelector));
            document.querySelector(`${modalSelector} .modal-body`).textContent =
                `¿Desea ${isActive ? 'desactivar' : 'activar'} ${entityName}?`;
            document.querySelector(confirmBtnSelector).onclick = async () => {
                await confirmPost(
                    `Confirmar acción`,
                    `${urlBase}/${isActive ? 'deactivate' : 'activate'}`,
                    { id },
                    {
                        onSuccess: () => location.reload(),
                        onError: err => notify.error('Error al cambiar estado')
                    }
                );
            };
            modal.show();
        });
    });
}
