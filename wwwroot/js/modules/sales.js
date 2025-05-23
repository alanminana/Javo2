
// wwwroot/js/modules/sales.js

import { ajaxGet, ajaxPost, notify, debug } from '../utils/app.js';
import { setupClienteSearch } from '../utils/client-search.js';
import { products } from '../components/products.js';
import { tables } from '../components/tables.js';
import { setupPaymentToggle } from '../utils/payment-toggle.js';
import { confirmPost } from '../utils/confirm-action.js';
import { serializeToJson, validateRequired } from '../utils/forms.js';

/**
 * Módulo unificado para manejar Ventas y Cotizaciones.
 */
const sales = {
    init() {
        // Cliente
        setupClienteSearch('#buscarCliente', '#DniCliente', {
            onSuccess: cliente => document.querySelector('#NombreCliente').value = cliente.nombre,
            onError: () => notify.error('Cliente no encontrado')
        });

        // Producto
        this.setupProductSearch('#buscarProductoBtn', '#ProductoCode');

        // Tabla de productos
        tables.init();

        // Forma de pago
        setupPaymentToggle('#FormaPagoID', 'payment-container');

        // Envío de formulario
        this.bindFormSubmit();
    },

    setupProductSearch(buttonSelector, inputSelector) {
        const btn = document.querySelector(buttonSelector);
        const input = document.querySelector(inputSelector);
        if (!btn || !input) return;

        btn.addEventListener('click', () => {
            const code = input.value.trim();
            if (!code) {
                notify.warning('Ingrese un código de producto');
                return;
            }
            products.searchByCode('/Ventas/BuscarProducto', code, {
                nameField: '#productoNombre',
                priceField: '#productoPrecio',
                quantityField: '#productoCantidad',
                onError: () => notify.error('Producto no encontrado')
            });
        });

        input.addEventListener('keypress', e => {
            if (e.key === 'Enter') {
                e.preventDefault();
                btn.click();
            }
        });
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
                debug('Error guardando:', err);
                notify.error('Error al guardar datos');
            }
        });
    }
};

document.addEventListener('DOMContentLoaded', () => sales.init());
export default sales;
