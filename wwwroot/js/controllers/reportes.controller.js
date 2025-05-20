// reportes.controller.js - Controlador para reportes y gráficos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.reportesController = {
        // Colores para los gráficos
        chartColors: {
            ventas: 'rgba(54,162,235,0.6)',
            productos: 'rgba(255,159,64,0.6)',
            clientes: 'rgba(75,192,192,0.6)',
            ganancias: 'rgba(153,102,255,0.6)',
            costos: 'rgba(255,99,132,0.6)'
        },

        init: function () {
            this.setupCharts();
            this.setupDateFilters();
            this.setupExportOptions();
        },

        // Configurar todos los gráficos
        setupCharts: function () {
            // Obtener referencias a los canvas
            const canvases = {
                ventas: document.getElementById('ventasChart'),
                productos: document.getElementById('productosChart'),
                clientes: document.getElementById('clientesChart'),
                ganancias: document.getElementById('gananciasChart')
            };

            // Inicializar gráficos que existan en la página
            if (canvases.ventas) this.initVentasChart(canvases.ventas);
            if (canvases.productos) this.initProductosChart(canvases.productos);
            if (canvases.clientes) this.initClientesChart(canvases.clientes);
            if (canvases.ganancias) this.initGananciasChart(canvases.ganancias);
        },

        // Inicializar un gráfico genérico
        initChart: function (canvas, type, labels, data, options) {
            if (!canvas) return null;

            // Destruir instancia previa si existe
            const existing = Chart.getChart(canvas);
            if (existing) {
                existing.destroy();
            }

            // Configuración por defecto
            const defaultOptions = {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: { mode: 'index', intersect: false }
                }
            };

            // Combinar opciones
            const chartOptions = $.extend(true, {}, defaultOptions, options);

            // Crear y retornar el gráfico
            const ctx = canvas.getContext('2d');
            return new Chart(ctx, {
                type: type,
                data: data,
                options: chartOptions
            });
        },

        // Inicializar gráfico de ventas
        initVentasChart: function (canvas) {
            const labels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'];
            const datasets = [{
                label: 'Ventas mensuales',
                data: [2000, 3000, 4000, 3500, 5000, 6000],
                backgroundColor: this.chartColors.ventas,
                borderColor: this.chartColors.ventas.replace('0.6', '1'),
                borderWidth: 2,
                tension: 0.4
            }];

            return this.initChart(canvas, 'line', labels, { labels, datasets });
        },

        // Inicializar gráfico de productos
        initProductosChart: function (canvas) {
            const labels = ['Prod A', 'Prod B', 'Prod C', 'Prod D', 'Prod E'];
            const datasets = [{
                label: 'Productos más vendidos',
                data: [120, 95, 80, 60, 45],
                backgroundColor: this.chartColors.productos,
                borderColor: this.chartColors.productos.replace('0.6', '1'),
                borderWidth: 1
            }];

            return this.initChart(canvas, 'bar', labels, { labels, datasets });
        },

        // Inicializar gráfico de clientes
        initClientesChart: function (canvas) {
            const labels = ['Nuevos', 'Recurrentes', 'Inactivos'];
            const datasets = [{
                label: 'Distribución de clientes',
                data: [35, 45, 20],
                backgroundColor: [
                    this.chartColors.clientes,
                    this.chartColors.ventas,
                    this.chartColors.productos
                ],
                borderWidth: 1
            }];

            return this.initChart(canvas, 'pie', labels, { labels, datasets });
        },

        // Inicializar gráfico de ganancias
        initGananciasChart: function (canvas) {
            const labels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'];
            const datasets = [
                {
                    label: 'Ganancias',
                    data: [1200, 1700, 2100, 1800, 2400, 2800],
                    backgroundColor: this.chartColors.ganancias,
                    borderColor: this.chartColors.ganancias.replace('0.6', '1'),
                    borderWidth: 2,
                    type: 'line',
                    yAxisID: 'y'
                },
                {
                    label: 'Costos',
                    data: [800, 1300, 1900, 1700, 2600, 3200],
                    backgroundColor: this.chartColors.costos,
                    borderColor: this.chartColors.costos.replace('0.6', '1'),
                    borderWidth: 1,
                    type: 'bar',
                    yAxisID: 'y'
                }
            ];

            const options = {
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
            };

            return this.initChart(canvas, 'bar', labels, { labels, datasets }, options);
        },

        // Configurar filtros de fecha
        setupDateFilters: function () {
            const self = this;

            $('#dateFilter').on('change', function () {
                const value = $(this).val();

                // Definir fechas según filtro
                let startDate, endDate;
                const today = new Date();

                switch (value) {
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
                        $('#dateRangeContainer').show();
                        return;
                    default:
                        // Por defecto, últimos 30 días
                        startDate = new Date(today);
                        startDate.setDate(startDate.getDate() - 30);
                        endDate = new Date(today);
                }

                // Ocultar selección personalizada
                $('#dateRangeContainer').hide();

                // Actualizar formulario
                $('#startDate').val(self.formatDate(startDate));
                $('#endDate').val(self.formatDate(endDate));

                // Actualizar reportes
                self.updateReports(startDate, endDate);
            });

            // Fechas personalizadas
            $('#startDate, #endDate').on('change', function () {
                const startDate = new Date($('#startDate').val());
                const endDate = new Date($('#endDate').val());

                if (!isNaN(startDate.getTime()) && !isNaN(endDate.getTime())) {
                    self.updateReports(startDate, endDate);
                }
            });

            // Establecer filtro predeterminado
            if ($('#dateFilter').length) {
                $('#dateFilter').val('month').trigger('change');
            }
        },

        // Formatear fecha para input date
        formatDate: function (date) {
            return date.toISOString().split('T')[0];
        },

        // Actualizar reportes basado en fechas
        updateReports: function (startDate, endDate) {
            // Ejemplo de implementación - en producción se haría AJAX
            // Aquí usamos datos de ejemplo
            App.ajax.get('/Reportes/GetData', {
                startDate: this.formatDate(startDate),
                endDate: this.formatDate(endDate)
            }, (data) => {
                // Actualizar los gráficos con los nuevos datos
                this.updateCharts(data);

                // Actualizar info de tarjetas resumen si existen
                if (data.summary) {
                    this.updateSummaryCards(data.summary);
                }
            });
        },

        // Actualizar gráficos con nuevos datos
        updateCharts: function (data) {
            // Ejemplo - en producción los datos vendrían del servidor
            if (data.ventas) {
                const ventasChart = Chart.getChart(document.getElementById('ventasChart'));
                if (ventasChart) {
                    ventasChart.data.labels = data.ventas.labels;
                    ventasChart.data.datasets[0].data = data.ventas.data;
                    ventasChart.update();
                }
            }

            if (data.productos) {
                const productosChart = Chart.getChart(document.getElementById('productosChart'));
                if (productosChart) {
                    productosChart.data.labels = data.productos.labels;
                    productosChart.data.datasets[0].data = data.productos.data;
                    productosChart.update();
                }
            }

            // Actualizar otros gráficos de manera similar
        },

        // Actualizar tarjetas de resumen
        updateSummaryCards: function (summary) {
            // Ejemplo - actualizar datos de tarjetas
            $('#totalVentas').text(App.format.currency(summary.totalVentas));
            $('#totalProductos').text(summary.totalProductos);
            $('#totalClientes').text(summary.totalClientes);
            $('#gananciaTotal').text(App.format.currency(summary.gananciaTotal));
        },

        // Configurar opciones de exportación
        setupExportOptions: function () {
            const self = this;

            $('#exportPDF').on('click', function () {
                self.exportReports('pdf');
            });

            $('#exportExcel').on('click', function () {
                self.exportReports('excel');
            });
        },

        // Exportar reportes
        exportReports: function (format) {
            const startDate = $('#startDate').val();
            const endDate = $('#endDate').val();

            if (!startDate || !endDate) {
                App.notify.warning('Seleccione un rango de fechas válido');
                return;
            }

            // Determinar URL según formato
            let url = '/Reportes/Export?format=' + format;
            url += '&startDate=' + startDate + '&endDate=' + endDate;

            // Redireccionar para descargar
            window.location.href = url;
        }
    };

    // Compatibilidad con código existente
    App.reportes = App.reportesController;

})(window, jQuery);