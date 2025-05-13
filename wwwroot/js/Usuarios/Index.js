
        document.addEventListener('DOMContentLoaded', function() {
            // Token para solicitudes AJAX
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Botones de cambio de estado
            document.querySelectorAll('.toggle-estado').forEach(btn => {
                btn.addEventListener('click', function() {
                    const id = this.dataset.id;
                    const estado = this.dataset.estado === 'true';
                    const username = this.closest('tr').querySelector('td:first-child').textContent;

                    // Configurar el modal
                    document.getElementById('estadoModalMessage').textContent =
                        `¿Está seguro que desea ${estado ? 'desactivar' : 'activar'} al usuario "${username}"?`;

                    // Configurar botón de confirmación
                    const confirmBtn = document.getElementById('confirmEstadoBtn');
                    confirmBtn.onclick = function() {
                        // Enviar solicitud AJAX
                        fetch(`@Url.Action("ToggleEstado", "Usuarios")/${id}`, {
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
                                const estadoCell = row.querySelector('td:nth-child(5)');
                                const toggleBtn = row.querySelector('.toggle-estado');

                                if (data.estado) {
                                    estadoCell.innerHTML = '<span class="badge bg-success">Activo</span>';
                                    toggleBtn.innerHTML = '<i class="bi bi-toggle-on"></i>';
                                    toggleBtn.title = 'Desactivar';
                                    toggleBtn.dataset.estado = 'true';
                                } else {
                                    estadoCell.innerHTML = '<span class="badge bg-danger">Inactivo</span>';
                                    toggleBtn.innerHTML = '<i class="bi bi-toggle-off"></i>';
                                    toggleBtn.title = 'Activar';
                                    toggleBtn.dataset.estado = 'false';
                                }
                            } else {
                                // Mostrar error
                                alert(data.message);
                            }
                        })
                        .catch(error => {
                            console.error('Error:', error);
                            alert('Ha ocurrido un error al cambiar el estado del usuario');
                        });
                    };

                    // Mostrar modal
                    const modal = new bootstrap.Modal(document.getElementById('estadoModal'));
                    modal.show();
                });
            });
        });
