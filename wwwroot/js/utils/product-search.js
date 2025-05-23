// utils/product-search.js
import { ajaxPost } from './app.js';
import { notify } from './app.js';

/**
 * Busca producto por código y poblá campos.
 */
export async function searchProductByCode(url, code, fields = {}) {
    try {
        const product = await ajaxPost(url, { code });

        if (fields.nameField) {
            const nameEl = document.querySelector(fields.nameField);
            if (nameEl) nameEl.value = product.nombre || '';
        }

        if (fields.priceField) {
            const priceEl = document.querySelector(fields.priceField);
            if (priceEl) priceEl.value = product.precio || 0;
        }

        if (fields.quantityField) {
            const qtyEl = document.querySelector(fields.quantityField);
            if (qtyEl) qtyEl.value = 1;
        }

        return product;
    } catch (e) {
        console.error('Error buscando producto:', e);
        if (fields.onError) fields.onError(e);
        else notify.error('Producto no encontrado');
        throw e;
    }
}

/**
 * Configura búsqueda automática con Enter en input.
 */
export function setupProductSearch(buttonSelector, inputSelector, url, fields = {}) {
    const btn = document.querySelector(buttonSelector);
    const input = document.querySelector(inputSelector);
    if (!btn || !input) return;

    const search = () => {
        const code = input.value.trim();
        if (!code) {
            notify.warning('Ingrese un código de producto');
            return;
        }
        searchProductByCode(url, code, fields);
    };

    btn.addEventListener('click', search);
    input.addEventListener('keypress', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            search();
        }
    });
}