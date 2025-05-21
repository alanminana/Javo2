import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';

// Estilos globales
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import 'toastr/build/toastr.min.css';

// Bootstrap JS
import 'bootstrap/dist/js/bootstrap.bundle.min.js';

// Crear la aplicación
const app = createApp(App);

// Registrar Pinia y Router
app.use(createPinia());
app.use(router);

// Montar la aplicación
app.mount('#app');