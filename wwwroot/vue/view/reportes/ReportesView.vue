<template>
    <div>
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Reportes</h2>
            <div>
                <div class="btn-group me-2">
                    <button type="button" class="btn btn-outline-primary" @click="exportarReporte('pdf')">
                        <i class="bi bi-file-pdf"></i> Exportar PDF
                    </button>
                    <button type="button" class="btn btn-outline-success" @click="exportarReporte('excel')">
                        <i class="bi bi-file-excel"></i> Exportar Excel
                    </button>
                </div>
            </div>
        </div>

        <!-- Filtros de fecha -->
        <div class="card bg-dark mb-4">
            <div class="card-header">
                <h5 class="mb-0">Filtros</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-3">
                        <div class="mb-3">
                            <label for="dateFilter" class="form-label">Período</label>
                            <select id="dateFilter" class="form-select bg-dark text-light" v-model="filtro.periodo" @change="cambiarPeriodo">
                                <option value="today">Hoy</option>
                                <option value="yesterday">Ayer</option>
                                <option value="week">Últimos 7 días</option>
                                <option value="month">Último mes</option>
                                <option value="quarter">Último trimestre</option>
                                <option value="year">Último año</option>
                                <option value="custom">Personalizado</option>
                            </select>
                        </div>
                    </div>
                    <div class="col-md-6" v-show="filtro.periodo === 'custom'">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="startDate" class="form-label">Fecha inicio</label>
                                    <input type="date" id="startDate" class="form-control bg-dark text-light" v-model="filtro.fechaInicio" @change="cargarReportes">
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="endDate" class="form-label">Fecha fin</label>
                                    <input type="date" id="endDate" class="form-control bg-dark text-light" v-model="filtro.fechaFin" @change="cargarReportes">
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3 d-flex align-items-end" v-show="filtro.periodo === 'custom'">
                        <div class="mb-3 w-100">
                            <button type="button" class="btn btn-primary w-100" @click="cargarReportes">
                                <i class="bi bi-arrow-repeat"></i> Actualizar
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tarjetas de resumen -->
        <div class="row mb-4">
            <div class="col-md-3">
                <div class="card bg-dark h-100">
                    <div class="card-body text-center">
                        <div class="display-6 mb-2">{{ resumen.totalVentas }}</div>
                        <h5 class="card-title">Total de Ventas</h5>
                        <p class="card-text text-success">
                            <i class="bi bi-graph-up-arrow me-1"></i>
                            {{ resumen.crecimientoVentas > 0 ? '+' : '' }}{{ resumen.crecimientoVentas }}%
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card bg-dark h-100">
                    <div class="card-body text-center">
                        <div class="display-6 mb-2">{{ resumen.totalProductos }}</div>
                        <h5 class="card-title">Productos Vendidos</h5>
                        <p class="card-text text-success">
                            <i class="bi bi-graph-up-arrow me-1"></i>
                            {{ resumen.crecimientoProductos > 0 ? '+' : '' }}{{ resumen.crecimientoProductos }}%
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card bg-dark h-100">
                    <div class="card-body text-center">
                        <div class="display-6 mb-2">{{ resumen.totalClientes }}</div>
                        <h5 class="card-title">Clientes Nuevos</h5>
                        <p class="card-text text-success">
                            <i class="bi bi-graph-up-arrow me-1"></i>
                            {{ resumen.crecimientoClientes > 0 ? '+' : '' }}{{ resumen.crecimientoClientes }}%
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card bg-dark h-100">
                    <div class="card-body text-center">
                        <div class="display-6 mb-2">{{ formatCurrency(resumen.gananciaTotal) }}</div>
                        <h5 class="card-title">Ganancia</h5>
                        <p :class="['card-text', resumen.crecimientoGanancias >= 0 ? 'text-success' : 'text-danger']">
                            <i :class="['bi me-1', resumen.crecimientoGanancias >= 0 ? 'bi-graph-up-arrow' : 'bi-graph-down-arrow']"></i>
                            {{ resumen.crecimientoGanancias > 0 ? '+' : '' }}{{ resumen.crecimientoGanancias }}%
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Gráficos -->
        <div class="row">
            <div class="col-md-8">
                <div class="card bg-dark mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">Ventas y Ganancias</h5>
                    </div>
                    <div class="card-body">
                        <canvas id="ventasChart" height="300"></canvas>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card bg-dark mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">Distribución de Clientes</h5>
                    </div>
                    <div class="card-body">
                        <canvas id="clientesChart" height="300"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <div class="card bg-dark mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">Productos más vendidos</h5>
                    </div>
                    <div class="card-body">
                        <canvas id="productosChart" height="300"></canvas>
                    </div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="card bg-dark mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">Ganancias vs Costos</h5>
                    </div>
                    <div class="card-body">
                        <canvas id="gananciasChart" height="300"></canvas>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { ref, onMounted, nextTick } from 'vue';
import Chart from 'chart.js/auto';
import apiService from '@/services/apiService';
import formatUtils from '@/utils/formatUtils';
import notificationUtils from '@/utils/notificationUtils';

export default {
  name: 'ReportesView',
  setup() {
    // Estado de los filtros
    const filtro = ref({
      periodo: 'month', // Por defecto último mes
      fechaInicio: '',
      fechaFin: ''
    });

    // Estado de datos
    const resumen = ref({
      totalVentas: 0,
      totalProductos: 0,
      totalClientes: 0,
      gananciaTotal: 0,
      crecimientoVentas: 0,
      crecimientoProductos: 0,
      crecimientoClientes: 0,
      crecimientoGanancias: 0
    });

    // Referencia a gráficos
    const charts = ref({
      ventas: null,
      productos: null,
      clientes: null,
      ganancias: null
    });

    // Datos para gráficos
    const chartData = ref({
      ventas: {
        labels: [],
        datasets: []
      },
      productos: {
        labels: [],
        datasets: []
      },
      clientes: {
        labels: [],
        datasets: []
      },
      ganancias: {
        labels: [],
        datasets: []
      }
    });

    // Colores para los gráficos
    const chartColors = {
      ventas: 'rgba(54,162,235,0.6)',
      productos: 'rgba(255,159,64,0.6)',
      clientes: 'rgba(75,192,192,0.6)',
      ganancias: 'rgba(153,102,255,0.6)',
      costos: 'rgba(255,99,132,0.6)'
    };

    onMounted(async () => {
      // Establecer fechas por defecto (último mes)
      cambiarPeriodo();

      // Cargar datos y crear gráficos
      await cargarReportes();
      initCharts();
    });

    function formatCurrency(value) {
      return formatUtils.currency(value);
    }

    function cambiarPeriodo() {
      const today = new Date();
      let startDate, endDate;

      switch (filtro.value.periodo) {
        case 'today':
          startDate = new Date(today);
          endDate = new Date(today);
          break;
        case 'yesterday':
          startDate = new Date(today);
          startDate.setDate(startDate.getDate() - 1);
          endDate = new Date(startDate);
          break;
        case 'week':
          startDate = new Date(today);
          startDate.setDate(startDate.getDate() - 7);
          endDate = new Date(today);
          break;
        case 'month':
          startDate = new Date(today);
          startDate.setMonth(startDate.getMonth() - 1);
          endDate = new Date(today);
          break;
        case 'quarter':
          startDate = new Date(today);
          startDate.setMonth(startDate.getMonth() - 3);
          endDate = new Date(today);
          break;
        case 'year':
          startDate = new Date(today);
          startDate.setFullYear(startDate.getFullYear() - 1);
          endDate = new Date(today);
          break;
        case 'custom':
          // No hacer nada, usar las fechas seleccionadas
          return;
      }

      // Formatear fechas para los inputs
      filtro.value.fechaInicio = formatDate(startDate);
      filtro.value.fechaFin = formatDate(endDate);

      // Cargar reportes con nuevas fechas
      if (filtro.value.periodo !== 'custom') {
        cargarReportes();
      }
    }

    function formatDate(date) {
      return date.toISOString().split('T')[0];
    }

    async function cargarReportes() {
      try {
        const response = await apiService.get('/Reportes/GetData', {
          startDate: filtro.value.fechaInicio,
          endDate: filtro.value.fechaFin
        });

        if (response.data) {
          // Actualizar resumen
          resumen.value = response.data.summary;

          // Actualizar datos de gráficos
          updateChartData(response.data);

          // Actualizar gráficos si ya están inicializados
          updateCharts();
        }
      } catch (err) {
        console.error('Error al cargar reportes:', err);
        notificationUtils.error('Error al cargar los datos de reportes');
      }
    }

    function updateChartData(data) {
      // Ventas
      if (data.ventas) {
        chartData.value.ventas = {
          labels: data.ventas.labels,
          datasets: [{
            label: 'Ventas',
            data: data.ventas.data,
            backgroundColor: chartColors.ventas,
            borderColor: chartColors.ventas.replace('0.6', '1'),
            borderWidth: 2,
            tension: 0.4
          }]
        };
      }

      // Productos
      if (data.productos) {
        chartData.value.productos = {
          labels: data.productos.labels,
          datasets: [{
            label: 'Productos más vendidos',
            data: data.productos.data,
            backgroundColor: chartColors.productos,
            borderColor: chartColors.productos.replace('0.6', '1'),
            borderWidth: 1
          }]
        };
      }

      // Clientes
      if (data.clientes) {
        chartData.value.clientes = {
          labels: data.clientes.labels,
          datasets: [{
            label: 'Distribución de clientes',
            data: data.clientes.data,
            backgroundColor: [
              chartColors.clientes,
              chartColors.ventas,
              chartColors.productos
            ],
            borderWidth: 1
          }]
        };
      }

      // Ganancias
      if (data.ganancias) {
        chartData.value.ganancias = {
          labels: data.ganancias.labels,
          datasets: [
            {
              label: 'Ganancias',
              data: data.ganancias.ganancias,
              backgroundColor: chartColors.ganancias,
              borderColor: chartColors.ganancias.replace('0.6', '1'),
              borderWidth: 2,
              type: 'line',
              yAxisID: 'y'
            },
            {
              label: 'Costos',
              data: data.ganancias.costos,
              backgroundColor: chartColors.costos,
              borderColor: chartColors.costos.replace('0.6', '1'),
              borderWidth: 1,
              type: 'bar',
              yAxisID: 'y'
            }
          ]
        };
      }
    }

    function initCharts() {
      // Inicializar gráficos con canvas
      nextTick(() => {
        // Gráfico de ventas
        const ventasCtx = document.getElementById('ventasChart')?.getContext('2d');
        if (ventasCtx) {
          charts.value.ventas = new Chart(ventasCtx, {
            type: 'line',
            data: chartData.value.ventas,
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
              }
            }
          });
        }

        // Gráfico de productos
        const productosCtx = document.getElementById('productosChart')?.getContext('2d');
        if (productosCtx) {
          charts.value.productos = new Chart(productosCtx, {
           type: 'bar',
            data: chartData.value.productos,
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
              }
            }
          });
        }

        // Gráfico de clientes
        const clientesCtx = document.getElementById('clientesChart')?.getContext('2d');
        if (clientesCtx) {
          charts.value.clientes = new Chart(clientesCtx, {
            type: 'pie',
            data: chartData.value.clientes,
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { position: 'bottom' }
              }
            }
          });
        }

        // Gráfico de ganancias
        const gananciasCtx = document.getElementById('gananciasChart')?.getContext('2d');
        if (gananciasCtx) {
          charts.value.ganancias = new Chart(gananciasCtx, {
            type: 'bar',
            data: chartData.value.ganancias,
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
              },
              scales: {
                y: {
                  type: 'linear',
                  display: true,
                  position: 'left',
                  title: {
                    display: true,
                    text: 'Monto ($)'
                  }
                }
              }
            }
          });
        }
      });
    }

    function updateCharts() {
      if (charts.value.ventas) {
        charts.value.ventas.data = chartData.value.ventas;
        charts.value.ventas.update();
      }

      if (charts.value.productos) {
        charts.value.productos.data = chartData.value.productos;
        charts.value.productos.update();
      }

      if (charts.value.clientes) {
        charts.value.clientes.data = chartData.value.clientes;
        charts.value.clientes.update();
      }

      if (charts.value.ganancias) {
        charts.value.ganancias.data = chartData.value.ganancias;
        charts.value.ganancias.update();
      }
    }

    function exportarReporte(formato) {
      if (!filtro.value.fechaInicio || !filtro.value.fechaFin) {
        notificationUtils.warning('Seleccione un rango de fechas válido');
        return;
      }

      // Construir URL para exportación
      let url = `/Reportes/Export?format=${formato}`;
      url += `&startDate=${filtro.value.fechaInicio}&endDate=${filtro.value.fechaFin}`;

      // Abrir en nueva ventana para descarga
      window.open(url, '_blank');
    }

    return {
      filtro,
      resumen,
      formatCurrency,
      cambiarPeriodo,
      cargarReportes,
      exportarReporte
    };
  }
};
</script>