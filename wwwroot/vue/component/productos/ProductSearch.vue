<template>
    <div class="row mb-3">
        <div class="col-md-12">
            <div class="card bg-dark">
                <div class="card-header">
                    <h5 class="mb-0">Búsqueda de producto</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="productoCodigo" class="form-label">Código o nombre</label>
                                <div class="input-group">
                                    <input type="text"
                                           id="productoCodigo"
                                           class="form-control bg-dark text-light"
                                           v-model="codigo"
                                           @keypress.enter.prevent="buscarProducto"
                                           placeholder="Ingrese código o nombre">
                                    <button type="button"
                                            class="btn btn-primary"
                                            @click="buscarProducto"
                                            :disabled="loading">
                                        <i class="bi" :class="loading ? 'bi-hourglass' : 'bi-search'"></i>
                                        Buscar
                                    </button>
                                </div>
                                <div v-if="error" class="text-danger mt-1">{{ error }}</div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="productoNombre" class="form-label">Nombre del producto</label>
                                <input type="text"
                                       id="productoNombre"
                                       class="form-control bg-dark text-light"
                                       v-model="productoActual.nombre"
                                       readonly>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="productoprecio" class="form-label">Precio</label>
                                <input type="text"
                                       id="productoPrice"
                                       class="form-control bg-dark text-light"
                                       v-model="precioFormateado"
                                       readonly>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="productoCantidad" class="form-label">Cantidad</label>
                                <input type="number"
                                       id="productoCantidad"
                                       class="form-control bg-dark text-light"
                                       v-model.number="cantidad"
                                       min="1"
                                       :disabled="!productoEncontrado">
                            </div>
                        </div>
                    </div>
                    <div class="text-end">
                        <button type="button"
                                class="btn btn-success"
                                @click="agregarProducto"
                                :disabled="!productoEncontrado || cantidad < 1">
                            <i class="bi bi-plus-circle"></i> Agregar producto
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal de producto no encontrado -->
    <div class="modal fade" id="productoNoEncontradoModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">Producto no encontrado</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>No se encontró ningún producto con el código o nombre especificado.</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { ref, computed, inject } from 'vue';
import { useProductStore } from '@/stores/productStore';
import formatUtils from '@/utils/formatUtils';
import { Modal } from 'bootstrap';

export default {
  name: 'ProductSearch',
  emits: ['product-added'],
  setup(props, { emit }) {
    const productStore = useProductStore();
    const codigo = ref('');
    const cantidad = ref(1);
    const error = ref('');
    const loading = ref(false);
    const productoActual = ref({
      id: 0,
      codigoAlfa: '',
      codigoBarra: '',
      nombre: '',
      marca: '',
      precio: 0,
      precioLista: 0
    });

    // Acceder al modal usando bootstrap
    let productoNoEncontradoModal = null;

    const productoEncontrado = computed(() => {
      return productoActual.value.id > 0;
    });

    const precioFormateado = computed(() => {
      return formatUtils.currency(productoActual.value.precio);
    });

    async function buscarProducto() {
      if (!codigo.value) {
        error.value = 'Ingrese un código o nombre para buscar';
        return;
      }

      loading.value = true;
      error.value = '';

      try {
        const producto = await productStore.searchProductByCode(codigo.value);

        if (producto) {
          productoActual.value = { ...producto };
          cantidad.value = 1;
        } else {
          resetProduct();
          // Mostrar modal
          if (!productoNoEncontradoModal) {
            productoNoEncontradoModal = new Modal(document.getElementById('productoNoEncontradoModal'));
          }
          productoNoEncontradoModal.show();
        }
      } catch (err) {
        error.value = 'Error al buscar el producto';
        console.error('Error al buscar producto:', err);
        resetProduct();
      } finally {
        loading.value = false;
      }
    }

    function agregarProducto() {
      if (!productoEncontrado.value) {
        error.value = 'Debe buscar un producto primero';
        return;
      }

      if (cantidad.value < 1) {
        error.value = 'La cantidad debe ser mayor a cero';
        return;
      }

      // Emitir evento con el producto y la cantidad
      emit('product-added', { ...productoActual.value }, cantidad.value);

      // Limpiar después de agregar
      resetProduct();
      codigo.value = '';
    }

    function resetProduct() {
      productoActual.value = {
        id: 0,
        codigoAlfa: '',
        codigoBarra: '',
        nombre: '',
        marca: '',
        precio: 0,
        precioLista: 0
      };
    }

    return {
      codigo,
      cantidad,
      error,
      loading,
      productoActual,
      productoEncontrado,
      precioFormateado,
      buscarProducto,
      agregarProducto
    };
  }
};
</script>