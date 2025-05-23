import { Chart } from 'chart.js';

/**
 * Inicia o actualiza un gráfico Chart.js en un canvas dado.
 * @param {string} selector - Selector del elemento canvas.
 * @param {object} config - Config de Chart.js (type, data, options).
 */
export function initChart(selector, config) {
    const canvas = document.querySelector(selector);
    if (!canvas) return;

    const existing = Chart.getChart(canvas);
    if (existing) existing.destroy();

    const ctx = canvas.getContext('2d');
    return new Chart(ctx, config);
}