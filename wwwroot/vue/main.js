import { createApp } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';
import { createPinia } from 'pinia';
import App from './App.vue';
import routes from './router';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';

// Crear instancia de Vue
const app = createApp(App);

// Configurar el router
const router = createRouter({
    history: createWebHistory(),
    routes
});

// Configurar Pinia
const pinia = createPinia();

// Registrar plugins
app.use(router);
app.use(pinia);

// Cargar permisos al inicio
import { usePermissionStore } from './stores/permissionStore';
const permissionStore = usePermissionStore();
permissionStore.loadPermissions();

// Montar la aplicación
app.mount('#app');