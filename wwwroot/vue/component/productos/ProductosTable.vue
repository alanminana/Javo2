<template>
    <div class="table-responsive">
        <table class="table table-dark table-hover" :id="tableId">
            <thead>
                <tr>
                    <th scope="col">Código</th>
                    <th scope="col">Producto</th>
                    <th scope="col">Cantidad</th>
                    <th scope="col">Precio</th>
                    <th scope="col">Subtotal</th>
                    <th scope="col" class="text-center">Acciones</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="(item, index) in items" :key="item.id" :data-index="index">
                    <td>
                        <input type="hidden" :name="`${prefix}[${index}].ProductoID`" :value="item.id">
                        <input type="hidden" :name="`${prefix}[${index}].CodigoAlfa`" :value="item.codigoAlfa">
                        <input type="hidden" :name="`${prefix}[${index}].CodigoBarra`" :value="item.codigoBarra">
                        <input type="hidden" :name="`${prefix}[${index}].Marca`" :value="item.marca">
                        <input type="hidden" :name="`${prefix}[${index}].NombreProducto`" :value="item.nombre">
                        <input type="hidden" :name="`${prefix}[${index}].PrecioUnitario`" :value="item.precio">
                        <input type="hidden" :name="`${prefix}[${index}].PrecioTotal`" :value="item.precio * item.cantidad">
                        <input type="hidden" :name="`${prefix}[${index}].PrecioLista`" :value="item.precioLista">
                        {{ item.codigoAlfa || item.codigoBarra || item.id }}
                    </td>
                    <td>{{ item.nombre }}</td>
                    <td>
                        <input type="number"
                               :name="`${prefix}[${index}].Cantidad`"
                               v-model.number="item.cantidad"
                               min="1"
                               class="form-control form-control-sm bg-dark text-light cantidad"
                               @change="updateQuantity(item.id, item.cantidad)">
                    </td>
                    <td>{{ formatCurrency(item.precio) }}</td>
                    <td><span class="subtotal">{{ formatCurrency(item.precio * item.cantidad) }}</span></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger" @click="removeItem(item.id)">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
                <tr v-if="items.length === 0">
                    <td colspan="6" class="text-center">No hay productos agregados</td>
                </tr>
            </tbody>
            <tfoot v-if="showTotals">
                <tr>
                    <td colspan="2" class="text-end fw-bold">Total:</td>
                    <td id="totalProductos">{{ totalItems }}</td>
                    <td></td>
                    <td id="totalVenta" colspan="2">{{ formatCurrency(totalAmount) }}</td>
                </tr>
            </tfoot>
        </table>
    </div>
</template>

<script>
import { computed } from 'vue';
import formatUtils from '@/utils/formatUtils';

export default {
  name: 'ProductosTable',
  props: {
    items: {
      type: Array,
      default: () => []
    },
    tableId: {
      type: String,
      default: 'productosTable'
    },
    prefix: {
      type: String,
      default: 'ProductosVenta'
    },
    showTotals: {
      type: Boolean,
      default: true
    }
  },
  setup(props, { emit }) {
    const totalItems = computed(() => {
      return props.items.reduce((total, item) => total + item.cantidad, 0);
    });

    const totalAmount = computed(() => {
      return props.items.reduce((total, item) => total + item.precio * item.cantidad, 0);
    });

    function formatCurrency(value) {
      return formatUtils.currency(value);
    }

    function updateQuantity(id, cantidad) {
      emit('update-quantity', id, cantidad);
    }

    function removeItem(id) {
      emit('remove-item', id);
    }

    return {
      totalItems,
      totalAmount,
      formatCurrency,
      updateQuantity,
      removeItem
    };
  }
};
</script>