// utils/bind-helpers.js

/**
 * Vincula tecla Enter a una función.
 */
export function bindEnter(selector, callback) {
    const el = document.querySelector(selector);
    if (!el) return;

    el.addEventListener('keypress', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            callback();
        }
    });
}

/**
 * Muestra modal con mensaje.
 */
export function showModal(selector, message) {
    const modal = document.querySelector(selector);
    if (!modal) return;

    const body = modal.querySelector('.modal-body');
    if (body) body.textContent = message;

    if (typeof bootstrap !== 'undefined') {
        new bootstrap.Modal(modal).show();
    }
}