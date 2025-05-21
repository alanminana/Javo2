// reportes.js - Módulo para reportes y gráficos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.reportes = {
        // Configuración de colores para diferentes tipos de gráficos
        chartColors: {
            ventas: 'rgba(54,162,235,0.6)',
            productos: 'rgba(255,159,64,0.6)',
            stock: 'rgba(75,192,192,0.6)',
            clientes: 'rgba(153,102,255,0.6)',
            ganancias: 'rgba(255,99,132,0.6)',
            costos: 'rgba(201,203,207,0.6)'
        },

        // Inicialización principal
        init: function () {
            console.log("Inicializando módulo de reportes");

            // Detectar contexto
            const hasDashboardCharts = $('#ventasChart, #productosChart, #stockChart').length > 0;
            const hasReportFilters = $('#reportFilters').length > 0;
            const hasExportButtons = $('.export-report').length > 0;

            // Inicializar componentes según contexto
            if (hasDashboardCharts) {
                this.setupCharts();
            }

            if (hasReportFilters) {
                this.setupReportFilters();
            }

            if (hasExportButtons) {
                this.setupExportHandlers();
            }
        },

        // Configurar y renderizar gráficos
        setupCharts: function () {
            // Obtener datos de gráficos desde elementos data
            const chartsData = this.getChartsData();

            // Inicializar cada gráfico encontrado en la página
            if ($('#ventasChart').length) {
                this.initChart('ventasChart', 'line', chartsData.ventas.labels,
                    chartsData.ventas.data, this.chartColors.ventas);
            }

            if ($('#productosChart').length) {
                this.initChart('productosChart', 'bar', chartsData.productos.labels,
                    chartsData.productos.data, this.chartColors.productos);
            }

            if ($('#stockChart').length) {
                this.initChart('stockChart', 'doughnut', chartsData.stock.labels,
                    chartsData.stock.data, Object.values(this.chartColors));
            }

            if ($('#clientesChart').length) {
                this.initChart('clientesChart', 'bar', chartsData.clientes.labels,
                    chartsData.clientes.data, this.chartColors.clientes);
            }

            if ($('#gananciasChart').length) {
                this.initChart('gananciasChart', 'line', chartsData.ganancias.labels,
                    chartsData.ganancias.data, this.chartColors.ganancias);
            }
        },

        // Extraer datos para gráficos desde elementos en el DOM
        getChartsData: function () {
            const data = {
                ventas: { labels: [], data: [] },
                productos: { labels: [], data: [] },
                stock: { labels: [], data: [] },
                clientes: { labels: [], data: [] },
                ganancias: { labels: [], data: [] }
            };

            // Intentar obtener datos desde elementos JSON
            try {
                // Ventas
                const ventasDataEl = document.getElementById('ventasData');
                if (ventasDataEl && ventasDataEl.textContent) {
                    const ventasData = JSON.parse(ventasDataEl.textContent);
                    data.ventas.labels = ventasData.map(item => item.periodo);
                    data.ventas.data = ventasData.map(item => item.valor);
                } else {
                    // Datos de demostración si no hay datos reales
                    data.ventas.labels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'];
                    data.ventas.data = [2000, 3000, 4000, 3500, 5000, 6000];
                }

                // Productos
                const productosDataEl = document.getElementById('productosData');
                if (productosDataEl && productosDataEl.textContent) {
                    const productosData = JSON.parse(productosDataEl.textContent);
                    data.productos.labels = productosData.map(item => item.nombre);
                    data.productos.data = productosData.map(item => item.cantidad);
                } else {
                    // Datos de demostración
                    data.productos.labels = ['Prod A', 'Prod B', 'Prod C', 'Prod D', 'Prod E'];
                    data.productos.data = [120, 95, 80, 60, 45];
                }

                // Stock
                const stockDataEl = document.getElementById('stockData');
                if (stockDataEl && stockDataEl.textContent) {
                    const stockData = JSON.parse(stockDataEl.textContent);
                    data.stock.labels = stockData.map(item => item.categoria);
                    data.stock.data = stockData.map(item => item.cantidad);
                } else {
                    // Datos de demostración
                    data.stock.labels = ['Disponible', 'Bajo stock', 'Agotado'];
                    data.stock.data = [65, 25, 10];
                }

                // Clientes
                const clientesDataEl = document.getElementById('clientesData');
                if (clientesDataEl && clientesDataEl.textContent) {
                    const clientesData = JSON.parse(clientesDataEl.textContent);
                    data.clientes.labels = clientesData.map(item => item.categoria);
                    data.clientes.data = clientesData.map(item => item.cantidad);
                } else {
                    // Datos de demostración
                    data.clientes.labels = ['Nuevos', 'Recurrentes', 'Inactivos'];
                    data.clientes.data = [30, 45, 25];
                }

                // Ganancias
                const gananciasDataEl = document.getElementById('gananciasData');
                if (gananciasDataEl && gananciasDataEl.textContent) {
                    const gananciasData = JSON.parse(gananciasDataEl.textContent);
                    data.ganancias.labels = gananciasData.map(item => item.periodo);
                    data.ganancias.data = gananciasData.map(item => item.valor);
                } else {
                    // Datos de demostración
                    data.ganancias.labels = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'];
                    data.ganancias.data = [500, 800, 1200, 900, 1500, 1800];
                }
            } catch (error) {
                console.error('Error al procesar datos de gráficos:', error);
                // Fallback a datos demo en caso de error
            }

            return data;
        },

        // Crear e inicializar un gráfico
        initChart: function (id, type, labels, data, bg) {
            const canvas = document.getElementById(id);
            if (!canvas) return;

            // Destruir instancia previa si existe
            const existing = Chart.getChart(canvas);
            if (existing) {
                existing.destroy();
            }

            const ctx = canvas.getContext('2d');

            // Determinar si es un gráfico de un solo color o múltiples colores
            let backgroundColor = bg;
            let borderColor = bg;

            if (Array.isArray(bg)) {
                // Para gráficos tipo pie/doughnut que necesitan array de colores
                backgroundColor = bg;
                borderColor = bg.map(color => color.replace('0.6', '1'));
            } else {
                // Para gráficos de un solo color (línea, barra)
                backgroundColor = bg;
                borderColor = bg.replace('0.6', '1');
            }

            // Configuración específica según tipo de gráfico
            let chartOptions = {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' }
                }
            };

            // Ajustes específicos según tipo
            if (type === 'line') {
                chartOptions.tension = 0.3; // Curvas suaves
                chartOptions.scales = {
                    y: { beginAtZero: true }
                };
            } else if (type === 'bar') {
                chartOptions.scales = {
                    y: { beginAtZero: true }
                };
            }

            // Crear el gráfico
            new Chart(ctx, {
                type: type,
                data: {
                    labels: labels,
                    datasets: [{
                        label: this.getChartLabel(id),
                        data: data,
                        backgroundColor: backgroundColor,
                        borderColor: borderColor,
                        borderWidth: 2
                    }]
                },
                options: chartOptions
            });
        },

        // Obtener etiqueta apropiada según ID del gráfico
        getChartLabel: function (chartId) {
            const labels = {
                'ventasChart': 'Ventas por período',
                'productosChart': 'Productos más vendidos',
                'stockChart': 'Estado del inventario',
                'clientesChart': 'Segmentación de clientes',
                'gananciasChart': 'Ganancias por período'
            };

            return labels[chartId] || 'Datos';
        },

        // Configurar filtros para reportes
        setupReportFilters: function () {
            const self = this;

            // Manejo de filtros de fecha
            $('#dateRangeFilter').on('change', function () {
                const range = $(this).val();

                // Configurar fechas según rango seleccionado
                const today = new Date();
                let startDate = new Date();

                switch (range) {
                    case 'today':
                        startDate = new Date();
                        break;
                    case 'yesterday':
                        startDate = new Date();
                        startDate.setDate(startDate.getDate() - 1);
                        break;
                    case 'week':
                        startDate.setDate(startDate.getDate() - 7);
                        break;
                    case 'month':
                        startDate.setMonth(startDate.getMonth() - 1);
                        break;
                    case 'quarter':
                        startDate.setMonth(startDate.getMonth() - 3);
                        break;
                    case 'year':
                        startDate.setFullYear(startDate.getFullYear() - 1);
                        break;
                    case 'custom':
                        $('#customDateRange').removeClass('d-none');
                        return;
                    default:
                        break;
                }

                // Ocultar rango personalizado si no se seleccionó
                if (range !== 'custom') {
                    $('#customDateRange').addClass('d-none');
                }

                // Formato para las fechas
                const formatDate = function (date) {
                    return date.toISOString().split('T')[0]; // YYYY-MM-DD
                };

                // Establecer fechas en campos ocultos
                $('#startDate').val(formatDate(startDate));
                $('#endDate').val(formatDate(today));

                // Refrescar reporte automáticamente si no es custom
                if (range !== 'custom') {
                    self.refreshReport();
                }
            });

            // Botón para aplicar filtro personalizado
            $('#applyCustomRange').on('click', function () {
                self.refreshReport();
            });

            // Botón para refrescar reporte
            $('#refreshReport').on('click', function () {
                self.refreshReport();
            });

            // Cambio de tipo de reporte
            $('#reportType').on('change', function () {
                // Mostrar/ocultar filtros específicos según tipo
                const reportType = $(this).val();
                $('.report-specific-filter').addClass('d-none');
                $(`.${reportType}-filters`).removeClass('d-none');

                // Refrescar reporte
                self.refreshReport();
            });
        },

        // Refrescar reporte con filtros aplicados
        refreshReport: function () {
            // Recolectar todos los filtros
            const filters = {
                reportType: $('#reportType').val(),
                startDate: $('#startDate').val(),
                endDate: $('#endDate').val(),
                clients: $('#clientFilter').val(),
                products: $('#productFilter').val(),
                categories: $('#categoryFilter').val(),
                // Otros filtros específicos...
            };

            // Mostrar indicador de carga
            $('#reportContent').html('<div class="text-center p-5"><div class="spinner-border"></div><p class="mt-2">Generando reporte...</p></div>');

            // Realizar solicitud AJAX
            $.ajax({
                url: '/Reportes/GenerarReporte',
                type: 'GET',
                data: filters,
                success: function (response) {
                    // Actualizar contenido del reporte
                    $('#reportContent').html(response);

                    // Inicializar gráficos si hay alguno
                    const chartCanvas = document.querySelector('#reportContent canvas');
                    if (chartCanvas) {
                        App.reportes.setupCharts();
                    }
                },
                error: function () {
                    $('#reportContent').html('<div class="alert alert-danger">Error al generar el reporte. Intente nuevamente.</div>');
                }
            });
        },

        // Configurar manejadores para exportación
        setupExportHandlers: function () {
            $('.export-report').on('click', function () {
                const format = $(this).data('format');
                const reportId = $('#currentReportId').val();

                // Recolectar filtros actuales
                const filters = {
                    reportType: $('#reportType').val(),
                    startDate: $('#startDate').val(),
                    endDate: $('#endDate').val(),
                    // Otros filtros relevantes...
                };

                // Preparar URL para descarga
                let url = `/Reportes/ExportarReporte?formato=${format}&reporteId=${reportId}`;

                // Añadir filtros a la URL
                for (const key in filters) {
                    if (filters[key]) {
                        url += `&${key}=${encodeURIComponent(filters[key])}`;
                    }
                }

                // Redirigir para descarga
                window.location.href = url;
            });
        },

        // Generar PDF a partir de elemento HTML
        generatePDF: function (elementId, filename) {
            const element = document.getElementById(elementId);
            if (!element) {
                console.error('Elemento no encontrado:', elementId);
                return;
            }

            // Usar html2pdf si está disponible
            if (typeof html2pdf !== 'undefined') {
                const opt = {
                    margin: 10,
                    filename: filename || 'reporte.pdf',
                    image: { type: 'jpeg', quality: 0.98 },
                    html2canvas: { scale: 2 },
                    jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
                };

                html2pdf().set(opt).from(element).save();
            } else {
                console.error('La librería html2pdf no está disponible');
                alert('No se puede generar el PDF. Contacte al administrador.');
            }
        },

        // Exportar a Excel
        exportToExcel: function (tableId, filename) {
            const table = document.getElementById(tableId);
            if (!table) {
                console.error('Tabla no encontrada:', tableId);
                return;
            }

            // Usar tableToExcel si está disponible
            if (typeof XLSX !== 'undefined') {
                const workbook = XLSX.utils.table_to_book(table);
                XLSX.writeFile(workbook, filename || 'reporte.xlsx');
            } else {
                console.error('La librería XLSX no está disponible');
                alert('No se puede exportar a Excel. Contacte al administrador.');
            }
        }
    };

    // Inicializar automáticamente si el DOM está listo
    $(function () {
        App.reportes.init();
    });

})(window, jQuery);