// modules/sales.js
import { ajaxPost, notify } from '../utils/app.js';
import { setupClienteSearch } from '../utils/client-search.js';
import { setupProductSearch } from '../utils/product-search.js';
import { tables } from './Tables.js';
import { setupPaymentToggle } from '../utils/payment-toggle.js';
import { serializeToJson, validateRequired } from '../utils/forms.js';

const sales = {
    init() {
        // Cliente
        setupClienteSearch('#buscarCliente', '#DniCliente', {
            onSuccess: cliente => {
                const nombreEl = document.querySelector('#NombreCliente');
                if (nombreEl) nombreEl.value = cliente.nombre;
            },
            onError: () => notify.error('Cliente no encontrado')
        });

        // Producto
        setupProductSearch('#buscarProductoBtn', '#ProductoCode', '/Ventas/BuscarProducto', {
            nameField: '#productoNombre',
            priceField: '#productoPrecio',
            quantityField: '#productoCantidad',
            onError: () => notify.error('Producto no encontrado')
        });

        // Tabla de productos
        tables.init();

        // Forma de pago
        setupPaymentToggle('#FormaPagoID', 'payment-container');

        // Envío de formulario
        this.bindFormSubmit();
    },

    bindFormSubmit() {
        const form = document.querySelector('#salesForm');
        if (!form) return;

        form.addEventListener('submit', async e => {
            e.preventDefault();
            if (!validateRequired('#salesForm')) {
                notify.warning('Complete los campos requeridos');
                return;
            }

            const data = serializeToJson('#salesForm');
            try {
                const isCotizacion = form.dataset.type === 'cotizacion';
                const url = isCotizacion ? '/Cotizaciones/Save' : '/Ventas/Save';
                await ajaxPost(url, data);
                notify.success((isCotizacion ? 'Cotización' : 'Venta') + ' guardada correctamente');
                form.reset();
                tables.updateTotals(document.querySelector('table'));
            } catch (err) {
                console.error('Error guardando:', err);
                notify.error('Error al guardar datos');
            }
        });
    }
};

document.addEventListener('DOMContentLoaded', () => sales.init());
export default sales;