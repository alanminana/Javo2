/**
 * Serializa un formulario a JSON.
 * @param {string} formSelector - Selector del <form>.
 * @returns {Object}
 */
export function serializeToJson(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return {};
    const data = {};
    new FormData(form).forEach((value, key) => {
        data[key] = value;
    });
    return data;
}

/**
 * Valida campos requeridos en un formulario.
 * @param {string} formSelector - Selector del <form>.
 * @returns {boolean} - true si todos los campos con 'required' están llenos.
 */
export function validateRequired(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return false;
    let valid = true;
    form.querySelectorAll('[required]').forEach(el => {
        if (!el.value.trim()) {
            el.classList.add('is-invalid');
            valid = false;
        } else {
            el.classList.remove('is-invalid');
        }
    });
    return valid;
}

