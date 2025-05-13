// reportes.js - Módulo para reportes, auditoría y funcionalidades adicionales
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    // Módulo de reportes
    App.reportes = {
        init: function () {
            // Animaciones en tarjetas de reportes
            $('.hover-lift').on('mouseenter', function () {
                $(this).css({
                    'transform': 'translateY(-5px)',
                    'box-shadow': '0 8px 16px rgba(0,0,0,0.2)'
                });
            }).on('mouseleave', function () {
                $(this).css({
                    'transform': 'translateY(0)',
                    'box-shadow': '0 4px 8px rgba(0,0,0,0.1)'
                });
            });
        },

        // Generar gráficos básicos
        setupCharts: function () {
            const colors = {
                ventas: 'rgba(54,162,235,0.6)',
                productos: 'rgba(255,159,64,0.6)'
            };

            // Crear gráfico genérico
            const initChart = (id, type, labels, data, bg) => {
                const ctx = document.getElementById(id).getContext('2d');
                if (!ctx) return;

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

            // Inicializar gráficos si existen los contenedores
            if (document.getElementById('ventasChart')) {
                initChart(
                    'ventasChart',
                    'line',
                    ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
                    [2000, 3000, 4000, 3500, 5000, 6000],
                    colors.ventas
                );
            }

            if (document.getElementById('productosChart')) {
                initChart(
                    'productosChart',
                    'bar',
                    ['Prod A', 'Prod B', 'Prod C', 'Prod D', 'Prod E'],
                    [120, 95, 80, 60, 45],
                    colors.productos
                );
            }
        }
    };

    // Módulo de auditoría
    App.auditoria = {
        init: function () {
            // Función helper para parsear detalles de auditoría
            this.setupParsing();
        },

        // Formatear detalle de auditoría
        setupParsing: function () {
            // Esta función toma un detalle en formato delimitado y lo convierte a HTML
            window.ParseDetalleToHtml = function (detalle) {
                if (!detalle) return '';

                var parts = detalle.split('|', StringSplitOptions.RemoveEmptyEntries);
                var html = "<ul>";

                parts.forEach(function (p) {
                    var sub = p.split(':');
                    if (sub.length < 2) return;

                    var prodID = sub[0];
                    var cambios = sub[1].split(';');

                    html += `<li><strong>ProductoID ${prodID}:</strong><ul>`;

                    cambios.forEach(function (c) {
                        var eq = c.split('=');
                        if (eq.length < 2) return;

                        var campo = eq[0];
                        var vals = eq[1].split("->");
                        if (vals.length < 2) return;

                        html += `<li>${campo}: ${vals[0]} → ${vals[1]}</li>`;
                    });

                    html += "</ul></li>";
                });

                html += "</ul>";
                return html;
            };
        }
    };

    // Módulo para Dashboard/Home
    App.dashboard = {
        init: function () {
            this.loadCotizacionesDolar();
        },

        // Cargar cotizaciones de dólar
        loadCotizacionesDolar: function () {
            const container = document.getElementById('card-container');
            if (!container) return;

            container.innerHTML = '';

            fetch('https://dolarapi.com/v1/dolares')
                .then(resp => resp.json())
                .then(data => {
                    data.slice(0, 2).forEach(d => {
                        const card = document.createElement('div');
                        card.className = 'card bg-secondary text-dark p-3 shadow-sm';
                        card.style.minWidth = '180px';
                        card.innerHTML = `
                            <div class="card-body text-center text-dark">
                                <h6 class="card-title mb-2">${d.nombre}</h6>
                                <p class="mb-1">Compra: <strong>${d.compra}</strong></p>
                                <p class="mb-1">Venta: <strong>${d.venta}</strong></p>
                                <small>${new Date(d.fechaActualizacion).toLocaleString()}</small>
                            </div>`;
                        container.appendChild(card);
                    });
                })
                .catch(() => {
                    container.innerHTML = '<p class="text-danger">No se pudieron cargar las cotizaciones.</p>';
                });
        }
    };

})(window, jQuery);