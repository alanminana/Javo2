// app.js - Punto de entrada principal de la aplicación
import './utils/format';
import './utils/validation';
import './utils/ajax-helper';
import './utils/notification';
import './utils/permission-handler';
import './utils/event-handler';

// Componentes
import './components/tables';
import './components/products';

// Controladores
import './controllers/cliente.controller';
import './controllers/producto.controller';
import './controllers/proveedor.controller';
import './controllers/venta.controller';
import './controllers/reportes.controller';
import './controllers/usuario-permiso.controller';

// Módulos
import './modules/transaction-form';
import './modules/catalogo-producto';

// Inicialización
document.addEventListener('DOMContentLoaded', function () {
    // Configuración global
    window.App.config = {
        debug: false,
        apiBaseUrl: '',
        currency: 'ARS',
        dateFormat: 'DD/MM/YYYY',
        version: '2.0.0'
    };

    // Inicializar aplicación
    window.App.init();
});