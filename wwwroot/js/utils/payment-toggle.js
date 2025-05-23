
// wwwroot/js/utils/payment-toggle.js

/**
 * Configura alternancia de contenedores de pago.
 * @param {string} toggleSelector - Selector del elemento <select> de forma de pago.
 * @param {string} containerPrefix - Prefijo de clase para contenedores (ej: 'payment-container').
 */
export function setupPaymentToggle(toggleSelector, containerPrefix) {
    const select = document.querySelector(toggleSelector);
    if (!select) return;

    const toggle = () => {
        const value = select.value;
        document.querySelectorAll(`.${containerPrefix}`).forEach(el => {
            el.style.display = el.dataset.type === value ? 'block' : 'none';
        });
    };

    select.addEventListener('change', toggle);
    // Estado inicial
    toggle();
}
