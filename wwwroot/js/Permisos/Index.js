
        document.addEventListener('DOMContentLoaded', function() {
            // Token para solicitudes AJAX
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Botones de cambio de estado
            document.querySelectorAll('.toggle-estado').forEach(btn => {
                btn.addEventListener('click', function() {
                    if (this.disabled) return;

                    const id = this.dataset.id;
                    const estado = this.dataset.estado === 'true';
                    const nombrePermiso = this.closest('tr').querySelector('td:first-child').textContent;

                    // Configurar el modal
                    document.getElementById('estadoModalMessage').textContent =
                        `¿Está seguro que desea ${estado ? 'desactivar' : 'activar'} el permiso "${nombrePermiso}"?`;

                    // Configurar botón de confirmación
                    const confirmBtn = document.getElementById('confirmEstadoBtn');
                    confirmBtn.onclick = function() {
                        // Enviar solicitud AJAX
                        fetch(`@Url.Action("ToggleEstado", "Permisos")/${id}`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                                'RequestVerificationToken': token
                            }
                        })
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
                                // Cerrar modal
                                bootstrap.Modal.getInstance(document.getElementById('estadoModal')).hide();

                                // Mostrar notificación
                                const alert = document.createElement('div');
                                alert.className = 'alert alert-success alert-dismissible fade show';
                                alert.innerHTML = `<i class="bi bi-check-circle me-2"></i> ${data.message}
                                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button>`;
                                document.querySelector('.container-fluid').prepend(alert);

                                // Actualizar UI
                                const row = document.querySelector(`[data-id="${id}"]`).closest('tr');
                                const estadoCell = row.querySelector('td:nth-child(4)');
                                const toggleBtn = row.querySelector('.toggle-estado');

                                if (data.estado) {
                                    estadoCell.innerHTML = '<span class="badge bg-success">Activo</span>';
                                    toggleBtn.innerHTML = '<i class="bi bi-toggle-on"></i>';
                                    toggleBtn.title = 'Desactivar';
                                    toggleBtn.dataset.estado = 'true';
                                    toggleBtn.classList.remove('btn-outline-success');
                                    toggleBtn.classList.add('btn-outline-warning');
                                } else {
                                    estadoCell.innerHTML = '<span class="badge bg-danger">Inactivo</span>';
                                    toggleBtn.innerHTML = '<i class="bi bi-toggle-off"></i>';
                                    toggleBtn.title = 'Activar';
                                    toggleBtn.dataset.estado = 'false';
                                    toggleBtn.classList.remove('btn-outline-warning');
                                    toggleBtn.classList.add('btn-outline-success');
                                }
                            } else {
                                // Mostrar error
                                alert(data.message);
                            }
                        })
                        .catch(error => {
                            console.error('Error:', error);
                            alert('Ha ocurrido un error al cambiar el estado del permiso');
                        });
                    };

                    // Mostrar modal
                    const modal = new bootstrap.Modal(document.getElementById('estadoModal'));
                    modal.show();
                });
            });

            // Funcionalidad de búsqueda
            const searchInput = document.getElementById('searchPermiso');
            const clearSearchBtn = document.getElementById('clearSearch');

            if (searchInput) {
                searchInput.addEventListener('input', function() {
                    const searchValue = this.value.toLowerCase();

                    // Buscar en todas las filas
                    document.querySelectorAll('.permiso-row').forEach(row => {
                        const content = row.textContent.toLowerCase();
                        const visible = content.includes(searchValue);
                        row.style.display = visible ? '' : 'none';
                    });

                    // Mostrar/ocultar grupos según si tienen filas visibles
                    document.querySelectorAll('.accordion-item').forEach(item => {
                        const grupo = item.querySelector('.accordion-collapse');
                        const visibleRows = grupo.querySelectorAll('.permiso-row[style="display: none;"]').length <
                                           grupo.querySelectorAll('.permiso-row').length;

                        if (searchValue && visibleRows) {
                            // Expandir el grupo si hay resultados y hay texto en la búsqueda
                            bootstrap.Collapse.getOrCreateInstance(grupo).show();
                        }
                    });
                });

                clearSearchBtn.addEventListener('click', function() {
                    searchInput.value = '';
                    searchInput.dispatchEvent(new Event('input'));

                    // Colapsar todos los grupos
                    document.querySelectorAll('.accordion-collapse.show').forEach(grupo => {
                        bootstrap.Collapse.getOrCreateInstance(grupo).hide();
                    });
                });
            }
        });
