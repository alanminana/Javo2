export default {
    isValidEmail(email) {
        const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    },

    validatePassword(password) {
        return {
            longitud: password.length >= 6,
            letraNumero: /[a-zA-Z]/.test(password) && /[0-9]/.test(password),
            caracterEspecial: /[^a-zA-Z0-9]/.test(password)
        };
    },

    isValidDNI(dni) {
        const cleaned = dni.toString().replace(/\D/g, '');
        return cleaned.length >= 7 && cleaned.length <= 8 && !isNaN(cleaned);
    },

    isValidPhone(phone) {
        const cleaned = phone.toString().replace(/\D/g, '');
        return cleaned.length >= 8;
    }
};