<template>
    <div class="card bg-dark mb-4">
        <div class="card-header">
            <h5 class="mb-0">Forma de Pago</h5>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <label for="FormaPagoID" class="form-label">Método de pago</label>
                <select id="FormaPagoID"
                        class="form-select bg-dark text-light"
                        v-model="formaPagoSeleccionada">
                    <option v-for="forma in formasPago"
                            :key="forma.id"
                            :value="forma.id">
                        {{ forma.nombre }}
                    </option>
                </select>
            </div>

            <!-- Contenedor de efectivo -->
            <div v-if="formaPagoSeleccionada === 1" class="payment-container">
                <div class="alert alert-info">
                    El pago en efectivo se realizará al momento de la entrega.
                </div>
            </div>

            <!-- Contenedor de tarjeta de crédito -->
            <div v-if="formaPagoSeleccionada === 2" class="payment-container">
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="NumeroTarjeta" class="form-label">Número de Tarjeta</label>
                            <input type="text"
                                   id="NumeroTarjeta"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.numeroTarjeta"
                                   placeholder="XXXX-XXXX-XXXX-XXXX">
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="TitularTarjeta" class="form-label">Titular</label>
                            <input type="text"
                                   id="TitularTarjeta"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.titularTarjeta"
                                   placeholder="Nombre como aparece en la tarjeta">
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <div class="mb-3">
                            <label for="FechaVencimiento" class="form-label">Vencimiento</label>
                            <input type="text"
                                   id="FechaVencimiento"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.fechaVencimiento"
                                   placeholder="MM/AA">
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="mb-3">
                            <label for="CodigoSeguridad" class="form-label">Código de Seguridad</label>
                            <input type="text"
                                   id="CodigoSeguridad"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.codigoSeguridad"
                                   placeholder="CVC">
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="mb-3">
                            <label for="Cuotas" class="form-label">Cuotas</label>
                            <select id="Cuotas"
                                    class="form-select bg-dark text-light"
                                    v-model="datosPago.cuotas">
                                <option value="1">1 cuota</option>
                                <option value="3">3 cuotas</option>
                                <option value="6">6 cuotas</option>
                                <option value="12">12 cuotas</option>
                                <option value="18">18 cuotas</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Contenedor de transferencia -->
            <div v-if="formaPagoSeleccionada === 4" class="payment-container">
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="ComprobanteTransferencia" class="form-label">Número de Comprobante</label>
                            <input type="text"
                                   id="ComprobanteTransferencia"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.comprobante"
                                   placeholder="Número de operación">
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="BancoOrigen" class="form-label">Banco</label>
                            <input type="text"
                                   id="BancoOrigen"
                                   class="form-control bg-dark text-light"
                                   v-model="datosPago.banco"
                                   placeholder="Banco de origen">
                        </div>
                    </div>
                </div>
            </div>

            <!-- Contenedor de crédito personal -->
            <div v-if="formaPagoSeleccionada === 6" class="payment-container">
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="Cuotas" class="form-label">Número de Cuotas</label>
                            <select id="Cuotas"
                                    class="form-select bg-dark text-light"
                                    v-model="datosPago.cuotas"
                                    @change="calcularRecargo">
                                <option value="1">1 cuota</option>
                                <option value="3">3 cuotas</option>
                                <option value="6">6 cuotas</option>
                                <option value="12">12 cuotas</option>
                            </select>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div v-if="datosPago.cuotas > 1" class="alert alert-info">
                            <p class="mb-0">Recargo por financiación: {{ datosPago.porcentajeRecargo }}%</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { ref, watch } from 'vue';

export default {
  name: 'FormaPago',
  emits: ['update:formaPago', 'update:datosPago'],
  props: {
    modelValue: {
      type: Number,
      default: 1
    }
  },
  setup(props, { emit }) {
    const formaPagoSeleccionada = ref(props.modelValue);
    const formasPago = ref([
      { id: 1, nombre: 'Efectivo' },
      { id: 2, nombre: 'Tarjeta de Crédito' },
      { id: 3, nombre: 'Tarjeta de Débito' },
      { id: 4, nombre: 'Transferencia' },
      { id: 5, nombre: 'Pago Virtual' },
      { id: 6, nombre: 'Crédito Personal' },
      { id: 7, nombre: 'Cheque' }
    ]);

    const datosPago = ref({
      numeroTarjeta: '',
      titularTarjeta: '',
      fechaVencimiento: '',
      codigoSeguridad: '',
      cuotas: '1',
      comprobante: '',
      banco: '',
      porcentajeRecargo: 0
    });

    function calcularRecargo() {
      const cuotas = parseInt(datosPago.value.cuotas);
      let porcentaje = 0;

      if (cuotas <= 3) {
        porcentaje = 10;
      } else if (cuotas <= 6) {
        porcentaje = 15;
      } else if (cuotas <= 12) {
        porcentaje = 20;
      } else {
        porcentaje = 25;
      }

      datosPago.value.porcentajeRecargo = porcentaje;
    }

    // Emitir cambios en la forma de pago
    watch(formaPagoSeleccionada, (newValue) => {
      emit('update:formaPago', newValue);
    });

    // Emitir cambios en datos de pago
    watch(datosPago, (newValue) => {
      emit('update:datosPago', newValue);
    }, { deep: true });

    return {
      formaPagoSeleccionada,
      formasPago,
      datosPago,
      calcularRecargo
    };
  }
};
</script>