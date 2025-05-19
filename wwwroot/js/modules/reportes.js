// reportes.js - Módulo para reportes y gráficos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.reportes = {
        setupCharts: function () {
            const colors = {
                ventas: 'rgba(54,162,235,0.6)',
                productos: 'rgba(255,159,64,0.6)'
            };

            const initChart = (id, type, labels, data, bg) => {
                const canvas = document.getElementById(id);
                if (!canvas) return;

                // **Destruye instancia previa si existe**
                const existing = Chart.getChart(canvas);
                if (existing) {
                    existing.destroy();
                }

                const ctx = canvas.getContext('2d');
                new Chart(ctx, {
                    type,
                    data: {
                        labels,
                        datasets: [{
                            data,
                            backgroundColor: bg,
                            borderColor: bg.replace('0.6', '1'),
                            borderWidth: 2
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { position: 'bottom' }
                        }
                    }
                });
            };

            initChart('ventasChart', 'line', ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'], [2000, 3000, 4000, 3500, 5000, 6000], colors.ventas);
            initChart('productosChart', 'bar', ['Prod A', 'Prod B', 'Prod C', 'Prod D', 'Prod E'], [120, 95, 80, 60, 45], colors.productos);
        },

        init: function () {
            // Solo corre una vez por carga de página
            this.setupCharts();
        }
    };

    // Arranca cuando el DOM esté listo
    $(function () {
        App.reportes.init();
    });

})(window, jQuery);
