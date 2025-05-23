// modules/operations.js
import { ajaxPost, notify } from '../utils/app.js';
import { bindEnter } from '../utils/bind-helpers.js';
import { setupProductSearch } from '../utils/product-search.js';
import { tables } from './Tables.js';
import { setupPaymentToggle } from '../utils/payment-toggle.js';
import { confirmPost } from '../utils/confirm-action.js';
import { serializeToJson, validateRequired } from '../utils/forms.js';

const operations = {
    init() {
        this.initCompra();
        this.initDevolucion();
        this.initEntrega();
    },

    initCompra() {
        const form = document.querySelector('#compraForm');
        if (!form) return;

        setupProductSearch('#buscarProducto', '#productoCodigo', '/Proveedores/BuscarProducto', {
            nameField: '#productoNombre',
            priceField: '#productoPrecio',
            quantityField: '#productoCantidad'
        });

        setupPaymentToggle('#FormaPagoID', 'payment-container');
        tables.init();

        form.addEventListener('submit', async e => {
            e.preventDefault();
            if (!validateRequired('#compraForm')) return notify.warning('Complete los campos requeridos');

            const data = serializeToJson('#compraForm');
            try {
                await ajaxPost('/Compras/Save', data);
                notify.success('Compra guardada');
                form.reset();
                tables.updateTotals(document.querySelector('#productosTable'));
            } catch (e) {
                console.error('Error compra', e);
                notify.error('Error al guardar compra');
            }
        });
    },

    initDevolucion() {
        const btn = document.querySelector('#btnBuscarVenta');
        const input = document.querySelector('#buscarVenta');
        if (!btn || !input) return;

        bindEnter('#buscarVenta', () => btn.click());
        btn.addEventListener('click', async () => {
            const numero = input.value.trim();
            if (!numero) return this.showDevolError('Ingrese factura válida');

            try {
                const res = await ajaxPost('/DevolucionGarantia/BuscarVenta', { numeroFactura: numero });
                if (res.success) {
                    this.renderDevolTable(res.items);
                } else {
                    this.showDevolError(res.message);
                }
            } catch {
                this.showDevolError('Error al buscar venta');
            }
        });
    },

    renderDevolTable(items) {
        const tbody = document.querySelector('#tablaProductos tbody');
        if (!tbody) return;

        tbody.innerHTML = items.map((item, i) => `
            <tr>
                <td>${item.codigo}</td>
                <td>${item.nombre}</td>
                <td>${item.cantidad}</td>
                <td>${item.precio}</td>
                <td><input type="checkbox" name="items[${i}].selected" value="${item.id}"></td>
            </tr>
        `).join('');

        tables.init();
    },

    showDevolError(msg) {
        const errorEl = document.querySelector('#errorBusqueda');
        if (errorEl) {
            errorEl.textContent = msg;
            errorEl.classList.remove('d-none');
        }
    },

    initEntrega() {
        document.querySelectorAll('.mark-delivered').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = btn.dataset.id;
                confirmPost('Confirmar entrega', '/Ventas/MarcarEntregada', { id }, {
                    onSuccess: () => location.reload(),
                    onError: () => notify.error('Error en entrega')
                });
            });
        });
    }
};

document.addEventListener('DOMContentLoaded', () => operations.init());
export default operations;