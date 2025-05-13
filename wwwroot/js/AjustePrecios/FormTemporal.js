document.addEventListener('DOMContentLoaded', function () {
    // Verificar disponibilidad de jQuery
    if (typeof jQuery === 'undefined') {
        console.error('jQuery no está disponible.');
        return;
    }

    // Funciones para el cálculo de duración
    function actualizarDuracion() {
        const fechaInicio = new Date(document.getElementById('FechaInicio').value);
        const fechaFin = new Date(document.getElementById('FechaFin').value);

        if (!isNaN(fechaInicio) && !isNaN(fechaFin)) {
            const diff = Math.ceil((fechaFin - fechaInicio) / (1000 * 60 * 60 * 24));
            document.getElementById('duracion').value = diff > 0 ? diff : 0;
        } else {
            document.getElementById('duracion').value = '';
        }
    }

    // Listeners para actualizar duración
    document.getElementById('FechaInicio').addEventListener('change', actualizarDuracion);
    document.getElementById('FechaFin').addEventListener('change', actualizarDuracion);

    // Calcular duración inicial
    actualizarDuracion();

    // Seleccionar/deseleccionar todos
    document.getElementById('checkAll').addEventListener('change', function () {
        const isChecked = this.checked;
        document.querySelectorAll('.producto-check').forEach(function (checkbox) {
            checkbox.checked = isChecked;
        });
    });

    // Simulación de ajuste
    document.getElementById('btnSimular').addEventListener('click', function () {
        // Obtener productos seleccionados
        const productosSeleccionados = Array.from(
            document.querySelectorAll('.producto-check:checked')
        ).map(cb => parseInt(cb.dataset.id));

        if (!productosSeleccionados.length) {
            alert('Debe seleccionar al menos un producto para simular.');
            return;
        }

        // Verificar porcentaje
        const porcentaje = parseFloat(document.getElementById('Porcentaje').value);
        if (isNaN(porcentaje) || porcentaje <= 0 || porcentaje > 100) {
            alert('Ingrese un porcentaje válido entre 0.01 y 100.');
            return;
        }

        // Verificar fechas
        const fechaInicio = new Date(document.getElementById('FechaInicio').value);
        const fechaFin = new Date(document.getElementById('FechaFin').value);
        if (isNaN(fechaInicio) || isNaN(fechaFin)) {
            alert('Seleccione fechas válidas.');
            return;
        }
        if (fechaInicio >= fechaFin) {
            alert('La fecha de inicio debe ser anterior a la fecha de finalización.');
            return;
        }

        // Verificar tipo de ajuste
        const tipoAjuste = document.getElementById('TipoAjuste').value;
        if (!tipoAjuste) {
            alert('Seleccione un motivo para el ajuste.');
            return;
        }

        // Preparar datos para la simulación
        const esAumento = document.getElementById('radioAumento').checked;
        const requestData = {
            ProductoIDs: productosSeleccionados,
            Porcentaje: porcentaje,
            EsAumento: esAumento,
            FechaInicio: fechaInicio,
            FechaFin: fechaFin,
            TipoAjuste: tipoAjuste
        };

        // Enviar solicitud AJAX para la simulación
        fetch('@Url.Action("SimularAjusteTemporal", "AjustePrecios")', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(requestData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Mostrar contenedor de simulación
                    document.getElementById('simulacionContainer').classList.remove('d-none');

                    // Actualizar resumen
                    document.getElementById('simTipoAjuste').textContent = data.tipoAjuste;
                    document.getElementById('simOperacion').textContent = esAumento ? 'AUMENTO' : 'DESCUENTO';
                    document.getElementById('simOperacion').className = esAumento ? 'badge bg-success' : 'badge bg-danger';
                    document.getElementById('simPorcentaje').textContent = porcentaje + '%';
                    document.getElementById('simFechaInicio').textContent = data.fechaInicio;
                    document.getElementById('simFechaFin').textContent = data.fechaFin;
                    document.getElementById('simDuracion').textContent = data.duracion;

                    // Construir tabla de simulación
                    const tbody = document.getElementById('simulacionBody');
                    tbody.innerHTML = '';

                    data.productos.forEach(p => {
                        const row = document.createElement('tr');
                        row.innerHTML = `
                                <td>${p.nombre}</td>
                                <td class="text-end">${p.costoActual.toFixed(2)}</td>
                                <td class="text-end">${p.contadoActual.toFixed(2)}</td>
                                <td class="text-end">${p.listaActual.toFixed(2)}</td>
                                <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.costoNuevo.toFixed(2)}</td>
                                <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.contadoNuevo.toFixed(2)}</td>
                                <td class="text-end ${esAumento ? 'text-success' : 'text-danger'}">${p.listaNuevo.toFixed(2)}</td>
                            `;
                        tbody.appendChild(row);
                    });

                    // Scroll hacia la simulación
                    document.getElementById('simulacionContainer').scrollIntoView({ behavior: 'smooth', block: 'start' });
                } else {
                    alert('Error al simular: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error en la solicitud:', error);
                alert('Error al procesar la simulación. Consulte la consola para más detalles.');
            });
    });
});