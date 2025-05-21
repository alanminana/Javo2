// Utilidades para notificaciones usando Bootstrap Toast
import { Toast } from 'bootstrap';

const notificationUtils = {
    /**
     * Muestra una notificación de éxito
     * @param {string} message - Mensaje a mostrar
     * @param {number} duration - Duración en milisegundos (por defecto 3000)
     */
    success(message, duration = 3000) {
        this._showToast(message, 'success', duration);
    },

    /**
     * Muestra una notificación de error
     * @param {string} message - Mensaje a mostrar
     * @param {number} duration - Duración en milisegundos (por defecto 5000)
     */
    error(message, duration = 5000) {
        this._showToast(message, 'danger', duration);
    },

    /**
     * Muestra una notificación de advertencia
     * @param {string} message - Mensaje a mostrar
     * @param {number} duration - Duración en milisegundos (por defecto 4000)
     */
    warning(message, duration = 4000) {
        this._showToast(message, 'warning', duration);
    },

    /**
     * Muestra una notificación informativa
     * @param {string} message - Mensaje a mostrar
     * @param {number} duration - Duración en milisegundos (por defecto 3000)
     */
    info(message, duration = 3000) {
        this._showToast(message, 'info', duration);
    },

    /**
     * Método interno para crear y mostrar un toast
     * @private
     */
    _showToast(message, type = 'success', duration = 3000) {
        // Crear elemento toast
        const toastElement = document.createElement('div');
        toastElement.className = `toast align-items-center text-bg-${type} border-0`;
        toastElement.setAttribute('role', 'alert');
        toastElement.setAttribute('aria-live', 'assertive');
        toastElement.setAttribute('aria-atomic', 'true');

        // Crear el cuerpo del toast
        const toastBody = document.createElement('div');
        toastBody.className = 'd-flex';

        // Contenido del toast
        const flexDiv = document.createElement('div');
        flexDiv.className = 'toast-body';
        flexDiv.textContent = message;

        // Botón de cierre
        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.className = 'btn-close btn-close-white me-2 m-auto';
        closeButton.setAttribute('data-bs-dismiss', 'toast');
        closeButton.setAttribute('aria-label', 'Cerrar');

        // Armar la estructura del toast
        toastBody.appendChild(flexDiv);
        toastBody.appendChild(closeButton);
        toastElement.appendChild(toastBody);

        // Obtener o crear el contenedor de toasts
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        // Añadir el toast al contenedor
        toastContainer.appendChild(toastElement);

        // Inicializar el toast con Bootstrap
        const toast = new Toast(toastElement, {
            autohide: true,
            delay: duration
        });

        // Mostrar el toast
        toast.show();

        // Eliminar el elemento cuando se oculte
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
};

export default notificationUtils;