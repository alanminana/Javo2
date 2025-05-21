import { defineStore } from 'pinia';
import apiService from '@/services/apiService';
import { useProductStore } from './productStore';

export const useSaleStore = defineStore('sales', {
    state: () => ({
        saleItems: [],
        cliente: {
            dni: '',
            nombre: '',
            telefono: '',
            domicilio: '',
            localidad: '',
            limiteCredito: 0,
            saldo: 0,
            saldoDisponible: 0
        },
        formaPago: 1, // Efectivo por defecto
        loading: false,
        error: null
    }),

    getters: {
        totalItems: (state) => state.saleItems.reduce((sum, item) => sum + item.cantidad, 0),

        totalAmount: (state) => state.saleItems.reduce((sum, item) => sum + (item.precio * item.cantidad), 0),

        formattedTotal: (state) => {
            return new Intl.NumberFormat('es-AR', {
                style: 'currency',
                currency: 'ARS'
            }).format(state.saleItems.reduce((sum, item) => sum + (item.precio * item.cantidad), 0));
        }
    },

    actions: {
        addProduct(product, cantidad = 1) {
            const existingIndex = this.saleItems.findIndex(item => item.id === product.id);

            if (existingIndex >= 0) {
                // Actualizar cantidad si ya existe
                this.saleItems[existingIndex].cantidad += cantidad;
            } else {
                // Agregar nuevo producto
                this.saleItems.push({
                    id: product.id,
                    codigoAlfa: product.codigoAlfa,
                    codigoBarra: product.codigoBarra,
                    nombre: product.nombre,
                    marca: product.marca,
                    precio: product.precio,
                    precioLista: product.precioLista,
                    cantidad: cantidad
                });
            }
        },

        removeProduct(productId) {
            this.saleItems = this.saleItems.filter(item => item.id !== productId);
        },

        updateQuantity(productId, cantidad) {
            const item = this.saleItems.find(item => item.id === productId);
            if (item && cantidad > 0) {
                item.cantidad = cantidad;
            }
        },

        async searchClientByDNI(dni) {
            if (!dni) return;

            this.loading = true;
            try {
                const response = await apiService.post('/Ventas/BuscarClientePorDNI', { dni });
                if (response.data.success) {
                    this.cliente = {
                        dni: dni,
                        nombre: response.data.data.nombre,
                        telefono: response.data.data.telefono,
                        domicilio: response.data.data.domicilio,
                        localidad: response.data.data.localidad,
                        limiteCredito: response.data.data.limiteCredito,
                        saldo: response.data.data.saldo,
                        saldoDisponible: response.data.data.saldoDisponible
                    };
                    this.error = null;
                } else {
                    this.error = response.data.message || 'Cliente no encontrado';
                    // Limpiar datos del cliente
                    this.cliente = { dni: dni, nombre: '', telefono: '', domicilio: '', localidad: '', limiteCredito: 0, saldo: 0, saldoDisponible: 0 };
                }
            } catch (error) {
                this.error = error.message || 'Error al buscar cliente';
                console.error('Error al buscar cliente:', error);
            } finally {
                this.loading = false;
            }
        },

        async saveSale(additionalData = {}) {
            if (this.saleItems.length === 0) {
                this.error = 'No hay productos en la venta';
                return { success: false, message: this.error };
            }

            if (!this.cliente.nombre) {
                this.error = 'Debe seleccionar un cliente';
                return { success: false, message: this.error };
            }

            this.loading = true;
            try {
                // Preparar datos de venta
                const saleData = {
                    ClienteDNI: this.cliente.dni,
                    NombreCliente: this.cliente.nombre,
                    FormaPagoID: this.formaPago,
                    PrecioTotal: this.totalAmount,
                    ProductosVenta: this.saleItems.map((item, index) => ({
                        ProductoID: item.id,
                        CodigoAlfa: item.codigoAlfa,
                        CodigoBarra: item.codigoBarra,
                        NombreProducto: item.nombre,
                        Marca: item.marca,
                        Cantidad: item.cantidad,
                        PrecioUnitario: item.precio,
                        PrecioTotal: item.precio * item.cantidad,
                        PrecioLista: item.precioLista
                    })),
                    ...additionalData
                };

                const response = await apiService.post('/Ventas/Create', saleData);
                if (response.data.success) {
                    // Limpiar estado tras venta exitosa
                    this.saleItems = [];
                    this.error = null;
                }
                return response.data;
            } catch (error) {
                this.error = error.message || 'Error al guardar la venta';
                console.error('Error al guardar la venta:', error);
                return { success: false, message: this.error };
            } finally {
                this.loading = false;
            }
        }
    }
});