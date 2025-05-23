// wwwroot/js/modules/inventory.js
import { ajaxGet, ajaxPost } from '../utils/app.js';
import { loadDropdown } from '../utils/dropdown.js';
import { bindEnter } from '../utils/utils.js';
import { confirmPost } from '../utils/confirm-action.js';
import { notify } from '../utils/app.js';

const inventory = {
    init() {
        this.initProductoForm();
        this.initCatalogo();
    },

    // ====================
    // PRODUCTO FORM
    // ====================
    initProductoForm() {
        const formEl = document.querySelector('#productoForm');
        if (!formEl) return;
        this.initProductoCategorias();
        bindEnter('#ProductoName', () => this.handleProductoSubmit());
        bindEnter('#ProductoCode', () => this.handleProductoSubmit());
        document.querySelector('#saveProducto')
            .addEventListener('click', () => this.handleProductoSubmit());
    },

    initProductoCategorias() {
        loadDropdown('/Productos/GetRubros', {}, '#rubroSelect', { placeholder: 'Seleccione rubro...' });
        document.querySelector('#rubroSelect')
            .addEventListener('change', e => {
                loadDropdown('/Productos/GetSubRubros', { rubroID: e.target.value }, '#subrubroSelect', { placeholder: 'Seleccione subrubro...' });
            });
        const initial = document.querySelector('#rubroSelect').value;
        if (initial) loadDropdown('/Productos/GetSubRubros', { rubroID: initial }, '#subrubroSelect', { placeholder: 'Seleccione subrubro...' });
    },

    async handleProductoSubmit() {
        const form = document.querySelector('#productoForm');
        if (!form) return;
        const data = {};
        new FormData(form).forEach((v, k) => data[k] = v);
        try {
            await ajaxPost(form.action, data);
            notify.success('Producto guardado correctamente');
            form.reset();
        } catch (e) {
            console.error('Error al guardar producto', e);
            notify.error('Error al guardar producto');
        }
    },

    // ====================
    // CATÁLOGO
    // ====================
    initCatalogo() {
        const tableEl = document.querySelector('#catalogoTable');
        const inputEl = document.querySelector('#CatalogoSearch');
        const btnEl = document.querySelector('#CatalogoSearchBtn');
        if (!tableEl || !inputEl || !btnEl) return;

        bindEnter('#CatalogoSearch', () => this.handleCatalogoSearch());
        btnEl.addEventListener('click', () => this.handleCatalogoSearch());
        document.addEventListener('click', e => {
            if (e.target.matches('.btn-delete')) {
                const id = e.target.getAttribute('data-id');
                confirmPost('¿Está seguro de eliminar este elemento?', '/Catalogo/Delete', { id }, {
                    onSuccess: () => { this.handleCatalogoSearch(); notify.success('Elemento eliminado'); },
                    onError: err => { console.error('Error al eliminar catálogo', err); notify.error('Error al eliminar'); }
                });
            }
        });

        // Opcionales: inicializar dropdowns en formulario de catálogo si existen
        this.initCatalogoFormDropdowns();
    },

    async handleCatalogoSearch() {
        const term = document.querySelector('#CatalogoSearch').value;
        try {
            const items = await ajaxGet('/Catalogo/Search', { term });
            this.renderCatalogoResults(items);
        } catch (e) {
            console.error('Error en búsqueda de catálogo', e);
            notify.error('Error en búsqueda');
        }
    },

    renderCatalogoResults(items) {
        const tbody = document.querySelector('#catalogoTable tbody');
        tbody.innerHTML = '';
        items.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${item.id}</td>
        <td>${item.nombre}</td>
        <td>
          <button class="btn btn-sm btn-primary btn-edit" data-id="${item.id}">Editar</button>
          <button class="btn btn-sm btn-danger btn-delete" data-id="${item.id}">Eliminar</button>
        </td>`;
            tbody.appendChild(tr);
        });
    },

    initCatalogoFormDropdowns() {
        const catSelect = document.querySelector('#categoriaSelect');
        if (!catSelect) return;
        loadDropdown('/Catalogo/GetCategorias', {}, '#categoriaSelect', { placeholder: 'Seleccione categoría...' });
        catSelect.addEventListener('change', e => {
            loadDropdown('/Catalogo/GetSubCategorias', { categoriaID: e.target.value }, '#subcategoriaSelect', { placeholder: 'Seleccione subcategoría...' });
        });
    }
};

// Auto-inicialización
document.addEventListener('DOMContentLoaded', () => inventory.init());
export default inventory;
