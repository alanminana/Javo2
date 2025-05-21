<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2>Productos</h2>
      <div>
        <button type="button" class="btn btn-outline-primary me-2" @click="openFilter">
          <i class="bi bi-filter"></i> Filtrar
        </button>
        <button type="button" class="btn btn-success" @click="crearProducto" v-if="tienePermiso('productos.crear')">
          <i class="bi bi-plus-circle"></i> Nuevo Producto
        </button>
      </div>
    </div>
    
    <!--Filtros -->
    <div: class="['card', 'bg-dark', 'mb-4', {'d-none': !filtrosVisibles}]" >
      <div class="card-header">
        <h5 class="mb-0">Filtros</h5>
      </div>
      <div class="card-body">
        <div class="row">
          <div class="col-md-3">
            <div class="mb-3">
              <label for="filterType" class="form-label">Filtrar por</label>
              <select id="filterType" class="form-select bg-dark text-light" v-model="filtro.tipo">
                <option value="Nombre">Nombre</option>
                <option value="Codigo">Código</option>
                <option value="Rubro">Rubro</option>
                <option value="SubRubro">SubRubro</option>
                <option value="Marca">Marca</option>
              </select>
            </div>
          </div>
          <div class="col-md-6">
            <div class="mb-3">
              <label for="filterValue" class="form-label">Valor</label>
              <input type="text" id="filterValue" class="form-control bg-dark text-light" v-model="filtro.valor" @keyup.enter="aplicarFiltro">
            </div>
          </div>
          <div class="col-md-3 d-flex align-items-end">
            <div class="mb-3 w-100">
              <button type="button" class="btn btn-primary me-2" @click="aplicarFiltro">
                <i class="bi bi-search"></i> Buscar
              </button>
              <button type="button" class="btn btn-outline-secondary" @click="limpiarFiltro">
                <i class="bi bi-x-circle"></i> Limpiar
              </button>
            </div>
          </div>
        </div >
      </div >
    </div >
    
    < !--Tabla de productos-- >
    <div class="card bg-dark">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-dark table-hover mb-0">
                    <thead>
                        <tr>
                            <th scope="col" width="5%">
                                <div class="form-check">
                                    <input type="checkbox" id="selectAll" class="form-check-input" v-model="selectAll" @change="toggleAll">
                                    <label class="form-check-label" for="selectAll">ID</label>
                                </div>
                            </th>
                            <th scope="col">Código</th>
                            <th scope="col">Nombre</th>
                            <th scope="col">Marca</th>
                            <th scope="col">Rubro</th>
                            <th scope="col">Stock</th>
                            <th scope="col">Precio</th>
                            <th scope="col" class="text-center">Acciones</th>
                        </tr>
                    </thead>
                    <tbody id="productosTableBody">
                        <tr v-for="producto in productos" :key="producto.id">
                        <td>
                            <div class="form-check">
                                <input type="checkbox" 
                           :id="`producto_${producto.id}`"
                                class="form-check-input producto-check"
                                :value="producto.id" 
                           v-model="seleccionados">
                                <label class="form-check-label" :for="`producto_${producto.id}`">
                                {{ producto.id }}
                            </label>
                        </div>
                    </td>
                    <td>{{ producto.codigoAlfa || producto.codigoBarra }}</td>
                    <td>{{ producto.nombre }}</td>
                    <td>{{ producto.marca }}</td>
                    <td>{{ producto.rubro }} / {{ producto.subRubro }}</td>
                    <td>{{ producto.stock }}</td>
                    <td>{{ formatCurrency(producto.precio) }}</td>
                    <td class="text-center">
<button type="button" class="btn btn-sm btn-outline-secondary me-1" @click="verProducto(producto.id)" title="Ver detalle">
                    <i class="bi bi-eye"></i>
                  </button>
                  <button type="button" class="btn btn-sm btn-outline-primary me-1" @click="editarProducto(producto.id)" v-if="tienePermiso('productos.editar')" title="Editar">
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button type="button" class="btn btn-sm btn-outline-danger" @click="eliminarProducto(producto.id)" v-if="tienePermiso('productos.eliminar')" title="Eliminar">
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
              <tr v-if="loading">
                <td colspan="8" class="text-center py-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                  </div>
                  <p class="mt-2">Cargando productos...</p>
                </td>
              </tr>
              <tr v-else-if="productos.length === 0">
                <td colspan="8" class="text-center py-4">
                  <p class="mb-0">No se encontraron productos</p>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    
    <!-- Acciones seleccionadas -->
    <div class="card bg-dark mt-3" v-if="seleccionados.length > 0">
      <div class="card-body">
        <h5 class="card-title">Acciones para {{ seleccionados.length }} productos seleccionados</h5>
        <div class="row">
          <div class="col-md-6">
            <div class="input-group mb-3">
              <input type="number" id="porcentaje" class="form-control bg-dark text-light" v-model="porcentaje" min="0.01" max="100" step="0.01" placeholder="Porcentaje">
              <div class="input-group-text bg-dark text-light">
                <div class="form-check form-check-inline">
                  <input class="form-check-input" type="radio" id="radioAumento" value="aumento" v-model="tipoAjuste" checked>
                  <label class="form-check-label" for="radioAumento">Aumento</label>
                </div>
                <div class="form-check form-check-inline">
                  <input class="form-check-input" type="radio" id="radioDescuento" value="descuento" v-model="tipoAjuste">
                  <label class="form-check-label" for="radioDescuento">Descuento</label>
                </div>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="input-group mb-3">
              <input type="text" id="descripcionAjuste" class="form-control bg-dark text-light" v-model="descripcionAjuste" placeholder="Descripción del ajuste">
              <button class="btn btn-primary" type="button" id="btnAdjustPrices" @click="ajustarPrecios">
                <i class="bi bi-graph-up-arrow"></i> Ajustar Precios
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Paginación -->
    <div class="d-flex justify-content-between align-items-center mt-3">
      <div>
        <span class="text-muted">Mostrando {{ productos.length }} de {{ totalProductos }} productos</span>
      </div>
      <nav aria-label="Paginación">
        <ul class="pagination pagination-sm">
          <li :class="['page-item', { disabled: paginaActual === 1 }]">
            <a class="page-link" href="#" @click.prevent="cambiarPagina(paginaActual - 1)">Anterior</a>
          </li>
          <li v-for="pagina in paginas" :key="pagina" :class="['page-item', { active: pagina === paginaActual }]">
            <a class="page-link" href="#" @click.prevent="cambiarPagina(pagina)">{{ pagina }}</a>
          </li>
          <li :class="['page-item', { disabled: paginaActual === totalPaginas }]">
            <a class="page-link" href="#" @click.prevent="cambiarPagina(paginaActual + 1)">Siguiente</a>
          </li>
        </ul>
      </nav>
    </div>
    
    <!-- Modal de confirmación para eliminar -->
    <div class="modal fade" id="confirmarEliminarModal" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content bg-dark text-light">
          <div class="modal-header">
            <h5 class="modal-title">Confirmar eliminación</h5>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <p>¿Está seguro que desea eliminar este producto? Esta acción no se puede deshacer.</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
            <button type="button" class="btn btn-danger" @click="confirmarEliminar">Eliminar</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Modal } from 'bootstrap';
import apiService from '@/services/apiService';
import notificationUtils from '@/utils/notificationUtils';
import formatUtils from '@/utils/formatUtils';
import { usePermissionStore } from '@/stores/permissionStore';

export default {
  name: 'ProductoList',
  setup() {
    const router = useRouter();
    const permissionStore = usePermissionStore();
    
    // Estado de la vista
    const productos = ref([]);
    const loading = ref(false);
    const error = ref('');
    const filtrosVisibles = ref(false);
    const filtro = ref({
      tipo: 'Nombre',
      valor: ''
    });
    const paginaActual = ref(1);
    const itemsPorPagina = ref(10);
    const totalProductos = ref(0);
    
    // Estado para selecciones
    const seleccionados = ref([]);
    const selectAll = ref(false);
    const productIdToDelete = ref(null);
    const porcentaje = ref('');
    const tipoAjuste = ref('aumento');
    const descripcionAjuste = ref('');
    
    // Referencia al modal de confirmación
    let confirmarEliminarModal = null;
    
    onMounted(() => {
      cargarProductos();
      
      // Inicializar modal
      confirmarEliminarModal = new Modal(document.getElementById('confirmarEliminarModal'));
      
      // Cargar permisos
      permissionStore.loadPermissions();
    });
    
    // Computed
    const totalPaginas = computed(() => {
      return Math.ceil(totalProductos.value / itemsPorPagina.value);
    });
    
    const paginas = computed(() => {
      const pages = [];
      const maxPages = 5;
      let startPage = Math.max(1, paginaActual.value - Math.floor(maxPages / 2));
      let endPage = Math.min(totalPaginas.value, startPage + maxPages - 1);
      
      // Ajustar startPage si endPage está en su límite
      if (endPage === totalPaginas.value) {
        startPage = Math.max(1, endPage - maxPages + 1);
      }
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
      return pages;
    });
    
    // Métodos
    function formatCurrency(value) {
      return formatUtils.currency(value);
    }
    
    function tienePermiso(permiso) {
      return permissionStore.hasPermission(permiso);
    }
    
    async function cargarProductos() {
      loading.value = true;
      error.value = '';
      
      try {
        const params = {
          page: paginaActual.value,
          pageSize: itemsPorPagina.value
        };
        
        // Agregar filtro si existe
        if (filtro.value.valor) {
          params[filtro.value.tipo] = filtro.value.valor;
        }
        
        const response = await apiService.get('/Productos/GetProducts', params);
        
        productos.value = response.data.productos;
        totalProductos.value = response.data.total;
        
        // Reiniciar selecciones
        seleccionados.value = [];
        selectAll.value = false;
      } catch (err) {
        error.value = 'Error al cargar productos';
        console.error('Error al cargar productos:', err);
        notificationUtils.error('Error al cargar productos');
      } finally {
        loading.value = false;
      }
    }
    
    function openFilter() {
      filtrosVisibles.value = !filtrosVisibles.value;
    }
    
    function aplicarFiltro() {
      paginaActual.value = 1;
      cargarProductos();
    }
    
    function limpiarFiltro() {
      filtro.value = {
        tipo: 'Nombre',
        valor: ''
      };
      aplicarFiltro();
    }
    
    function cambiarPagina(pagina) {
      if (pagina < 1 || pagina > totalPaginas.value) return;
      paginaActual.value = pagina;
      cargarProductos();
    }
    
    function crearProducto() {
      router.push('/Productos/Create');
    }
    
    function verProducto(id) {
      router.push(`/Productos/Details/${id}`);
    }
    
    function editarProducto(id) {
      router.push(`/Productos/Edit/${id}`);
    }
    
    function eliminarProducto(id) {
      productIdToDelete.value = id;
      confirmarEliminarModal.show();
    }
    
    async function confirmarEliminar() {
      if (!productIdToDelete.value) return;
      
      try {
        const response = await apiService.post(`/Productos/Delete/${productIdToDelete.value}`, {});
        
        if (response.data.success) {
          notificationUtils.success('Producto eliminado correctamente');
          confirmarEliminarModal.hide();
          cargarProductos();
        } else {
          notificationUtils.error(response.data.message || 'Error al eliminar el producto');
        }
      } catch (err) {
        console.error('Error al eliminar producto:', err);
        notificationUtils.error('Error al eliminar el producto');
      } finally {
        productIdToDelete.value = null;
      }
    }
    
    function toggleAll() {
      if (selectAll.value) {
        seleccionados.value = productos.value.map(p => p.id);
      } else {
        seleccionados.value = [];
      }
    }
    
    async function ajustarPrecios() {
      if (seleccionados.value.length === 0) {
        notificationUtils.warning('Seleccione al menos un producto para ajustar precios');
        return;
      }
      
      const porcentajeNum = parseFloat(porcentaje.value);
      if (isNaN(porcentajeNum) || porcentajeNum <= 0 || porcentajeNum > 100) {
        notificationUtils.warning('Ingrese un porcentaje válido entre 0.01 y 100');
        return;
      }
      
      if (!descripcionAjuste.value) {
        notificationUtils.warning('Ingrese una descripción para el ajuste');
        return;
      }
      
      const isAumento = tipoAjuste.value === 'aumento';
      
      if (!confirm(`¿Está seguro que desea ${isAumento ? 'aumentar' : 'reducir'} en un ${porcentajeNum}% los precios de ${seleccionados.value.length} productos?`)) {
        return;
      }
      
      try {
        const response = await apiService.post('/Productos/IncrementarPrecios', {
          ProductoIDs: seleccionados.value.join(','),
          porcentaje: porcentajeNum,
          isAumento: isAumento,
          descripcion: descripcionAjuste.value
        });
        
        if (response.data.success) {
          notificationUtils.success('Precios ajustados correctamente');
          cargarProductos();
          
          // Limpiar selección
          seleccionados.value = [];
          porcentaje.value = '';
          descripcionAjuste.value = '';
        } else {
          notificationUtils.error(response.data.message || 'Error al ajustar precios');
        }
      } catch (err) {
        console.error('Error al ajustar precios:', err);
        notificationUtils.error('Error al procesar el ajuste de precios');
      }
    }
    
    return {
      productos,
      loading,
      error,
      filtrosVisibles,
      filtro,
      paginaActual,
      totalProductos,
      seleccionados,
      selectAll,
      porcentaje,
      tipoAjuste,
      descripcionAjuste,
      totalPaginas,
      paginas,
      formatCurrency,
      tienePermiso,
      openFilter,
      aplicarFiltro,
      limpiarFiltro,
      cambiarPagina,
      crearProducto,
      verProducto,
      editarProducto,
      eliminarProducto,
      confirmarEliminar,
      toggleAll,
      ajustarPrecios
    };
  }
};
</script>