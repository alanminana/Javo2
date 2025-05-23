// wwwroot/js/modules/reportes.js
import { initChart } from '../utils/chart-helper.js';
import { ajaxGet } from '../utils/app.js';
import { notify } from '../utils/app.js';

const reportes = {
    async setupCharts() {
        try {
            const ventasData = await ajaxGet('/Reportes/GetVentasMensuales');
            initChart('#ventasChart', {
                type: 'line',
                data: { labels: ventasData.labels, datasets: [{ data: ventasData.values, backgroundColor: ventasData.bg, borderColor: ventasData.border, borderWidth: 2 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
            });
        } catch (e) {
            console.error('Error cargando datos de ventas:', e);
            notify.error('No se pudieron cargar los datos de ventas');
        }

        try {
            const productosData = await ajaxGet('/Reportes/GetRankingProductos');
            initChart('#productosChart', {
                type: 'bar',
                data: { labels: productosData.labels, datasets: [{ data: productosData.values, backgroundColor: productosData.bg, borderColor: productosData.border, borderWidth: 2 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
            });
        } catch (e) {
            console.error('Error cargando datos de productos:', e);
            notify.error('No se pudieron cargar los datos de productos');
        }
    },

    init() { document.addEventListener('DOMContentLoaded', () => this.setupCharts()); }
};

export default reportes;
