import axios from 'axios';

const apiClient = axios.create({
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    }
});

// Interceptor para incluir el token CSRF en cada solicitud
apiClient.interceptors.request.use(config => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        config.headers['RequestVerificationToken'] = token;
    }
    return config;
});

export default {
    get(url, params) {
        return apiClient.get(url, { params });
    },
    post(url, data) {
        return apiClient.post(url, data);
    },
    put(url, data) {
        return apiClient.put(url, data);
    },
    delete(url) {
        return apiClient.delete(url);
    }
};