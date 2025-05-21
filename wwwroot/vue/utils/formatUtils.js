export default {
    currency(value, currency = 'ARS', locale = 'es-AR') {
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value || 0);
    },

    date(date, format = 'DD/MM/YYYY') {
        if (!date) return '';
        const d = new Date(date);
        if (isNaN(d.getTime())) return '';

        if (format === 'locale') {
            return d.toLocaleDateString('es-AR');
        }

        const day = d.getDate().toString().padStart(2, '0');
        const month = (d.getMonth() + 1).toString().padStart(2, '0');
        const year = d.getFullYear();

        return format
            .replace('DD', day)
            .replace('MM', month)
            .replace('YYYY', year);
    },

    number(value, decimals = 2) {
        return new Intl.NumberFormat('es-AR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(value || 0);
    },

    percent(value, decimals = 2) {
        return new Intl.NumberFormat('es-AR', {
            style: 'percent',
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format((value || 0) / 100);
    }
};