
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


