<template>
    <div class="card bg-dark mb-4">
        <div class="card-header">
            <h5 class="mb-0">Datos del cliente</h5>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="DniCliente" class="form-label">DNI Cliente</label>
                        <div class="input-group">
                            <input type="text"
                                   id="DniCliente"
                                   class="form-control bg-dark text-light"
                                   v-model="dni"
                                   @keypress.enter.prevent="buscarCliente"
                                   placeholder="Ingrese DNI">
                            <button type="button"
                                    class="btn btn-primary"
                                    @click="buscarCliente"
                                    :disabled="loading">
                                <i class="bi" :class="loading ? 'bi-hourglass' : 'bi-search'"></i>
                                Buscar
                            </button>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="NombreCliente" class="form-label">Nombre</label>
                        <input type="text"
                               id="NombreCliente"
                               class="form-control bg-dark text-light"
                               v-model="cliente.nombre"
                               readonly>
                    </div>
                </div>
            </div>

            <div class="row" v-if="clienteEncontrado">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="TelefonoCliente" class="form-label">Teléfono</label>
                        <input type="text"
                               id="TelefonoCliente"
                               class="form-control bg-dark text-light"
                               v-model="cliente.telefono"
                               readonly>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="DomicilioCliente" class="form-label">Domicilio</label>
                        <input type="text"
                               id="DomicilioCliente"
                               class="form-control bg-dark text-light"
                               v-model="cliente.domicilio"
                               readonly>
                    </div>
                </div>
            </div>

            <div class="row" v-if="clienteEncontrado">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="LocalidadCliente" class="form-label">Localidad</label>
                        <input type="text"
                               id="LocalidadCliente"
                               class="form-control bg-dark text-light"
                               v-model="cliente.localidad"
                               readonly>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="CelularCliente" class="form-label">Celular</label>
                        <input type="text"
                               id="CelularCliente"
                               class="form-control bg-dark text-light"
                               v-model="cliente.celular"
                               readonly>
                    </div>
                </div>
            </div>

            <div class="row" v-if="clienteEncontrado && mostrarCredito">
                <div class="col-md-4">
                    <div class="mb-3">
                        <label for="LimiteCreditoCliente" class="form-label">Límite de Crédito</label>
                        <input type="text"
                               id="LimiteCreditoCliente"
                               class="form-control bg-dark text-light"
                               :value="formatCurrency(cliente.limiteCredito)"
                               readonly>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="mb-3">
                        <label for="SaldoCliente" class="form-label">Saldo</label>
                        <input type="text"
                               id="SaldoCliente"
                               class="form-control bg-dark text-light"
                               :value="formatCurrency(cliente.saldo)"
                               readonly>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="mb-3">
                        <label for="SaldoDisponibleCliente" class="form-label">Saldo Disponible</label>
                        <input type="text"
                               id="SaldoDisponibleCliente"
                               class="form-control bg-dark text-light"
                               :value="formatCurrency(cliente.saldoDisponible)"
                               readonly>
                    </div>
                </div>
            </div>

            <div class="alert alert-warning d-none" id="clienteNotFound" v-if="error">
                {{ error }}
            </div>
        </div>
    </div>
</template>

<script>import { ref, computed } from 'vue';
import apiService from '@/services/apiService';
import formatUtils from '@/utils/formatUtils';
import notificationUtils from '@/utils/notificationUtils';

export default {
  name: 'ClienteSearch',
  props: {
    mostrarCredito: {
      type: Boolean,
      default: true
    }
  },
  emits: ['cliente-found', 'cliente-not-found'],
  setup(props, { emit }) {
    const dni = ref('');
    const error = ref('');
    const loading = ref(false);
    const cliente = ref({
      nombre: '',
      telefono: '',
      domicilio: '',
      localidad: '',
      celular: '',
      limiteCredito: 0,
      saldo: 0,
      saldoDisponible: 0
    });

    const clienteEncontrado = computed(() => {
      return cliente.value.nombre !== '';
    });

    function formatCurrency(value) {
      return formatUtils.currency(value);
    }

    async function buscarCliente() {
      if (!dni.value) {
        error.value = 'Ingrese un DNI para buscar';
        emit('cliente-not-found', error.value);
        return;
      }

      loading.value = true;
      error.value = '';

      try {
        const response = await apiService.post('/Ventas/BuscarClientePorDNI', { dni: dni.value });

        if (response.data.success) {
          cliente.value = {
            nombre: response.data.data.nombre,
            telefono: response.data.data.telefono,
            domicilio: response.data.data.domicilio,
            localidad: response.data.data.localidad,
            celular: response.data.data.celular,
            limiteCredito: response.data.data.limiteCredito,
            saldo: response.data.data.saldo,
            saldoDisponible: response.data.data.saldoDisponible
          };
          emit('cliente-found', { dni: dni.value, ...cliente.value });
        } else {
          error.value = response.data.message || 'Cliente no encontrado';
          resetCliente();
          emit('cliente-not-found', error.value);
        }
      } catch (err) {
        error.value = 'Error al buscar cliente';
        console.error('Error al buscar cliente:', err);
        resetCliente();
        emit('cliente-not-found', error.value);
        notificationUtils.error(error.value);
      } finally {
        loading.value = false;
      }
    }

    function resetCliente() {
      cliente.value = {
        nombre: '',
        telefono: '',
        domicilio: '',
        localidad: '',
        celular: '',
        limiteCredito: 0,
        saldo: 0,
        saldoDisponible: 0
      };
    }

    return {
      dni,
      cliente,
      error,
      loading,
      clienteEncontrado,
      formatCurrency,
      buscarCliente
    };
  }
};</script>