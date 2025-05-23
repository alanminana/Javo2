// wwwroot/js/modules/operations.js
import { ajaxGet, ajaxPost, notify, debug } from '../utils/app.js';
import { bindEnter } from '../utils/utils.js';
import { products } from '../components/products.js';
import { tables } from '../components/tables.js';
import { setupPaymentToggle } from '../utils/payment-toggle.js';
import { confirmAction, confirmPost } from '../utils/confirm-action.js';
import { serializeToJson, validateRequired } from '../utils/forms.js';

const operations = {
    init() {
        this.initCompra();
        this.initDevolucion();
        this.initEntrega();
    },

    // -------- COMPRA --------
    initCompra() {
        const form = document.querySelector('#compraForm');
        if (!form) return;

        bindEnter('#productoCodigo', () => this.searchAndPopulateCompra());
        document.querySelector('#buscarProducto').addEventListener('click', () => this.searchAndPopulateCompra());

        setupPaymentToggle('#FormaPagoID', 'payment-container');

        this.setupTable('#productosTable', '#agregarProducto', this.handleAddCompra.bind(this));

        form.addEventListener('submit', async e => {
            e.preventDefault();
            if (!validateRequired('#compraForm')) return notify.warning('Complete los campos requeridos');
            const data = serializeToJson('#compraForm');
            try { await ajaxPost('/Compras/Save', data); notify.success('Compra guardada'); form.reset(); tables.updateTotals(document.querySelector('#productosTable')); } catch (e) { debug('Error compra', e); notify.error('Error al guardar compra'); }
        });
    },

    searchAndPopulateCompra() {
        const code = document.querySelector('#productoCodigo').value.trim();
        if (!code) return notify.warning('Ingrese un código');
        products.searchByCode('/Proveedores/BuscarProducto', code, {
            nameField: '#productoNombre', priceField: '#productoPrecio', quantityField: '#productoCantidad', onError: () => notify.error('Producto no encontrado')
        });
    },

    handleAddCompra() {
        // Usa products.addToTable y tables.init() internamente
        const prod = { id: /* from productosActual */, codigo: /*...*/ };
        products.addToTable('#productosTable', prod);
    },

    // ------ DEVOLUCIÓN ------
    initDevolucion() {
        const btn = document.querySelector('#btnBuscarVenta');
        const input = document.querySelector('#buscarVenta');
        if (btn && input) {
            bindEnter('#buscarVenta', () => btn.click());
            btn.addEventListener('click', async () => {
                const numero = input.value.trim(); if (!numero) return this.showDevolError('Ingrese factura válida');
                try {
                    const res = await ajaxPost('/DevolucionGarantia/BuscarVenta', { numeroFactura: numero });
                    if (res.success) { this.renderDevolTable(res.items); } else this.showDevolError(res.message);
                } catch { this.showDevolError('Error al buscar venta'); }
            });
        }
    },

    renderDevolTable(items) {
        const tbody = document.querySelector('#tablaProductos tbody'); tbody.innerHTML = '';
        items.forEach((item, i) => {
            // Reusar products.getRowTemplate y tables reindex/updateTotals
        });
        tables.init();
    },

    showDevolError(msg) { document.querySelector('#errorBusqueda').textContent = msg; document.querySelector('#errorBusqueda').classList.remove('d-none'); },

    // ------ ENTREGA ------
    initEntrega() {
        const btns = document.querySelectorAll('.mark-delivered');
        btns.forEach(btn => btn.addEventListener('click', () => {
            const id = btn.dataset.id;
            confirmPost('Confirmar entrega', '/Ventas/MarcarEntregada', { id }, {
                onSuccess: () => location.reload(), onError: () => notify.error('Error en entrega')
            });
        }));
    }
};

document.addEventListener('DOMContentLoaded', () => operations.init());
export default operations;