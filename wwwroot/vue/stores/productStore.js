import { defineStore } from 'pinia';
import apiService from '@/services/apiService';

export const useProductStore = defineStore('products', {
    state: () => ({
        products: [],
        currentProduct: null,
        loading: false,
        error: null
    }),

    actions: {
        async fetchProducts(filters = {}) {
            this.loading = true;
            try {
                const response = await apiService.get('/Productos/GetProducts', filters);
                this.products = response.data;
                this.error = null;
            } catch (error) {
                this.error = error.message || 'Error al cargar productos';
                console.error('Error al cargar productos:', error);
            } finally {
                this.loading = false;
            }
        },

        async searchProductByCode(code) {
            if (!code) return null;

            this.loading = true;
            try {
                const response = await apiService.post('/Productos/BuscarProducto', { codigoProducto: code });
                if (response.data.success) {
                    const product = response.data.data;
                    this.currentProduct = {
                        id: product.productoID,
                        codigoAlfa: product.codigoAlfa || '',
                        codigoBarra: product.codigoBarra || '',
                        nombre: product.nombreProducto,
                        marca: product.marca || '',
                        precio: parseFloat(product.precioUnitario) || 0,
                        precioLista: parseFloat(product.precioLista) || 0,
                        costo: parseFloat(product.precioCosto) || 0
                    };
                    return this.currentProduct;
                } else {
                    this.error = response.data.message || 'Producto no encontrado';
                    return null;
                }
            } catch (error) {
                this.error = error.message || 'Error al buscar producto';
                console.error('Error al buscar producto:', error);
                return null;
            } finally {
                this.loading = false;
            }
        }
    }
});