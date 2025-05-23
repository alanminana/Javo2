
// wwwroot/js/utils/auth.js
import { validatePasswordStrength } from './password-validator.js';

/**
 * Setup validación en tiempo real de campo contraseña.
 * @param {string} inputSelector
 * @param {string} feedbackSelector
 */
export function setupPasswordValidation(inputSelector, feedbackSelector) {
    const input = document.querySelector(inputSelector);
    const feedback = document.querySelector(feedbackSelector);
    if (!input || !feedback) return;

    input.addEventListener('input', () => {
        const valid = validatePasswordStrength(input.value);
        if (!valid) {
            feedback.textContent =
                'Debe tener al menos 8 caracteres, mayúscula, minúscula, número y símbolo.';
        } else {
            feedback.textContent = '';
        }
        input.classList.toggle('is-invalid', !valid);
    });
}

// Auto-init
document.addEventListener('DOMContentLoaded', () =>
    setupPasswordValidation('#Password', '#PasswordFeedback')
);
