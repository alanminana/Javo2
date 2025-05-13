    @section Scripts {
        <script type="module">
            document.addEventListener('DOMContentLoaded', async () => {
              // Mostrar solo 2 cotizaciones
              const container = document.getElementById('card-container');
              container.innerHTML = '';
              try {
                const resp = await fetch('https://dolarapi.com/v1/dolares');
                const data = await resp.json();
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
              } catch {
                container.innerHTML = '<p class="text-danger">No se pudieron cargar las cotizaciones.</p>';
              }

              // Configuración de gráficos
              const colors = { ventas: 'rgba(54,162,235,0.6)', productos: 'rgba(255,159,64,0.6)' };
              const initChart = (id, type, labels, data, bg) => {
                new Chart(document.getElementById(id).getContext('2d'), { type, data: { labels, datasets: [{ data, backgroundColor: bg, borderColor: bg.replace('0.6','1'), borderWidth: 2 }] }, options: { responsive:true, maintainAspectRatio:false, plugins:{ legend:{ position:'bottom'} } } });
              };
              initChart('ventasChart','line',['Ene','Feb','Mar','Abr','May','Jun'],[2000,3000,4000,3500,5000,6000],colors.ventas);
              initChart('productosChart','bar',['Prod A','Prod B','Prod C','Prod D','Prod E'],[120,95,80,60,45],colors.productos);
            });
        </script>
    }
