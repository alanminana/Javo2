import { ajaxGet, ajaxPost, notify } from '../utils/app.js';
import { loadDropdown } from '../utils/dropdown.js';
import { bindEnter } from '../utils/utils.js';
import { components as { products } } from '../components/products.js';
import { tables } from '../components/tables.js';

const providers = {
    init() {
        this.loadAssignedList('#assignedProducts');
        this.setupProductSearch();
        this.setupFilters('#applyFilter', '#filterField', '#filterValue', '/Proveedores/Filter', '#proveedoresTableBody');
    },

    loadAssignedList(selector) {
        // Extraer IDs existentes en lista o tabla
        this.assignedIds = Array.from(document.querySelectorAll(`${selector} [data-id]`))
            .map(el => parseInt(el.dataset.id));
    },

    setupProductSearch() {
        const input = '#productSearch';
        const btn = '#searchProductBtn';
        // Autocomplete (si usa plugin) u omitir
        bindEnter(input, () => this.onSearchProducts());
        document.querySelector(btn).addEventListener('click', () => this.onSearchProducts());
    },

    async onSearchProducts() {
        const term = document.querySelector('#productSearch').value.trim();
        if (term.length < 2) return notify.warning('Ingrese al menos 2 caracteres');
        try {
            const data = await ajaxPost('/Proveedores/SearchProductsForAssignment', { term });
            this.renderSearchResults(data, '#productResultsTable tbody', item => {
                this.addAssignedProduct(item.id, item.name, item.marca);
            });
        } catch {
            notify.error('Error al buscar productos');
        }
    },

    renderSearchResults(items, tbodySelector, onAssign) {
        const tbody = document.querySelector(tbodySelector);
        tbody.innerHTML = items.length
            ? items.map(item => `<tr data-id="${item.id}">
          <td>${item.name}</td>
          <td>${item.marca}</td>
          <td><button class="btn btn-sm btn-primary assign-product">Asignar</button></td>
        </tr>`).join('')
            : '<tr><td colspan="3">No se encontraron productos</td></tr>';
        tables.init();
        document.querySelectorAll(`${tbodySelector} .assign-product`).forEach(btn => {
            btn.addEventListener('click', e => {
                const row = e.target.closest('tr');
                const id = parseInt(row.dataset.id);
                if (!this.assignedIds.includes(id)) {
                    onAssign({ id, name: row.children[0].textContent, marca: row.children[1].textContent });
                    this.assignedIds.push(id);
                }
            });
        });
    },

    addAssignedProduct(id, name, marca) {
        // Reusar tables.addToTable o lógica similar
        const table = document.querySelector('#assignedProducts');
        tables.init();
    },

    setupFilters(btnSelector, fieldSel, valueSel, url, targetSel) {
        const btn = document.querySelector(btnSelector);
        bindEnter(valueSel, () => btn.click());
        btn.addEventListener('click', async () => {
            const filters = {
                filterField: document.querySelector(fieldSel).value,
                filterValue: document.querySelector(valueSel).value
            };
            try {
                const html = await ajaxGet(url, filters);
                document.querySelector(targetSel).innerHTML = html;
            } catch {
                notify.error('Error al aplicar filtros');
            }
        });
    }
};

document.addEventListener('DOMContentLoaded', () => providers.init());
export default providers;