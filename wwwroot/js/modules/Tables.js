// wwwroot/js/components/tables.js

/**
 * Componente para operaciones comunes en tablas de detalle de formularios.
 * Sustituye al antiguo Tables.js y al código duplicado de loadDropdown (el helper de dropdown reside en utils/dropdown.js).
 */
export const tables = {
    /**
     * Inicializa el componente: vincula eventos de eliminación, cambio de cantidad y selección en masa.
     */
    init() {
        this._initDeleteButtons();
        this._initQuantityChange();
        this._initSelectAll();
    },

    /** Elimina la fila y actualiza índices y totales */
    _initDeleteButtons() {
        document.addEventListener('click', e => {
            if (e.target.matches('.eliminar-producto, .remove-product, .btn-delete-row')) {
                const row = e.target.closest('tr');
                const table = row.closest('table');
                row.remove();
                this.reindexRows(table);
                this.updateTotals(table);
            }
        });
    },

    /** Recalcula subtotal y totales al cambiar cantidad */
    _initQuantityChange() {
        document.addEventListener('input', e => {
            if (e.target.matches('.cantidad, .item-quantity')) {
                const row = e.target.closest('tr');
                const priceCell = row.querySelector('.item-price, input[name$=".PrecioUnitario"]');
                const totalCell = row.querySelector('.item-total, input[name$=".PrecioTotal"]');
                const price = parseFloat(priceCell.textContent || priceCell.value) || 0;
                const qty = parseInt(e.target.value, 10) || 0;
                const subtotal = price * qty;

                // Actualizar subtotal en UI
                if (totalCell.tagName === 'INPUT') totalCell.value = subtotal;
                else totalCell.textContent = subtotal.toFixed(2);

                this.updateTotals(row.closest('table'));
            }
        });
    },

    /** Habilita checkbox "seleccionar todo" y resalta filas */
    _initSelectAll() {
        document.querySelectorAll('table[data-select-all]').forEach(table => {
            const master = table.querySelector('.select-all');
            const checkboxes = table.querySelectorAll('tbody input[type="checkbox"]');

            master?.addEventListener('change', () => {
                checkboxes.forEach(cb => {
                    cb.checked = master.checked;
                    cb.closest('tr').classList.toggle('table-active', master.checked);
                });
            });

            table.addEventListener('change', e => {
                if (e.target.matches('tbody input[type="checkbox"]')) {
                    e.target.closest('tr').classList.toggle('table-active', e.target.checked);
                    const allChecked = Array.from(checkboxes).every(cb => cb.checked);
                    master.checked = allChecked;
                }
            });
        });
    },

    /** Reindexa los índices y actualiza atributos name para arrays en formularios */
    reindexRows(table) {
        table.querySelectorAll('tbody tr').forEach((row, index) => {
            // Índice visible
            const idxCell = row.querySelector('.row-index');
            if (idxCell) idxCell.textContent = index + 1;

            // Actualizar atributos name con índices
            row.querySelectorAll('input[name]').forEach(input => {
                const name = input.getAttribute('name');
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                input.setAttribute('name', newName);
            });
        });
    },

    /** Suma totales y muestra en el pie de tabla */
    updateTotals(table) {
        let totalProducts = 0;
        let totalAmount = 0;

        table.querySelectorAll('tbody tr').forEach(row => {
            const qtyEl = row.querySelector('.cantidad, input[name$=".Cantidad"]');
            const priceEl = row.querySelector('.item-price, input[name$=".PrecioUnitario"]');
            const qty = parseInt(qtyEl.value || qtyEl.textContent, 10) || 0;
            const price = parseFloat(priceEl.value || priceEl.textContent) || 0;
            totalProducts += qty;
            totalAmount += price * qty;
        });

        // Mostrar totales
        const totalProductsEl = table.querySelector('.total-products') || document.getElementById('totalProductos');
        const totalAmountEl = table.querySelector('.total-amount') || document.getElementById('totalVenta');
        if (totalProductsEl) totalProductsEl.textContent = totalProducts;
        if (totalAmountEl) totalAmountEl.textContent = totalAmount.toFixed(2);

        return { totalProducts, totalAmount };
    }
};

// Auto-inicializar si hay tablas
document.addEventListener('DOMContentLoaded', () => tables.init());
