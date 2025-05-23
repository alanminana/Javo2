// wwwroot/js/utils/app.js (actualizado con debug completo)
export const config = {
    debug: false,
    apiBaseUrl: '',
    currency: 'ARS',
    dateFormat: 'DD/MM/YYYY'
};

// Formateo común
export const format = {
    currency: value => new Intl.NumberFormat('es-AR', { style: 'currency', currency: config.currency }).format(value),
    date: date => new Date(date).toLocaleDateString('es-AR')
};

// Notificaciones genéricas
export const notify = {
    success: message => (typeof toastr !== 'undefined' ? toastr.success(message) : alert(message)),
    error: message => (typeof toastr !== 'undefined' ? toastr.error(message) : alert('Error: ' + message)),
    warning: message => (typeof toastr !== 'undefined' ? toastr.warning(message) : alert('Advertencia: ' + message)),
    info: message => (typeof toastr !== 'undefined' ? toastr.info(message) : alert('Info: ' + message))
};

// Debug extendido
export function debug(message, data) {
    if (config.debug) console.log(message, data || '');
}

// Métodos adicionales para compatibilidad
debug.log = debug;
debug.error = (message, data) => { if (config.debug) console.error(message, data || ''); };
debug.init = () => { debug('Debug iniciado'); };

// AJAX genérico con token anti-forgery
export async function ajaxGet(url, params = {}) {
    const query = new URLSearchParams(params).toString();
    const response = await fetch(`${url}${query ? '?' + query : ''}`, { credentials: 'same-origin' });
    if (!response.ok) throw new Error(response.statusText);
    return response.json();
}

export async function ajaxPost(url, data) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const headers = { 'Content-Type': 'application/json', 'RequestVerificationToken': token };
    const response = await fetch(url, { method: 'POST', headers, body: JSON.stringify(data), credentials: 'same-origin' });
    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText);
    }
    return response.json();
}

// Inicialización global
document.addEventListener('DOMContentLoaded', () => {
    debug.init();
});