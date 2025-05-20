// usuario-permiso.controller.js - Controlador unificado para usuarios y permisos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.usuarioPermisoController = {
        init: function () {
            this.setupUsuariosHandlers();
            this.setupPermisosHandlers();
            this.setupRolesHandlers();

            // Inicializar formulario de permisos si está presente
            if (document.getElementById('Nombre') && document.getElementById('Codigo')) {
                this.setupCodigoGenerator();
            }
        },

        // Manejadores para Usuarios
        setupUsuariosHandlers: function () {
            // Filtrar usuarios
            document.getElementById('limpiarFiltro')?.addEventListener('click', function () {
                document.getElementById('Termino').value = '';
                document.getElementById('Activo').value = '';
                document.getElementById('RolID').value = '';
                document.getElementById('usuariosFilterForm').submit();
            });

            // Toggle estado usuario
            this.setupEstadoToggle('.toggle-estado', 'estadoModal', 'confirmEstadoBtn',
                '/Usuarios/ToggleEstado/', 'usuario');
        },

        // Manejadores para Permisos
        setupPermisosHandlers: function () {
            // Búsqueda de permisos
            const searchInput = document.getElementById('searchPermiso');
            const clearSearchBtn = document.getElementById('clearSearch');

            if (searchInput) {
                searchInput.addEventListener('input', function () {
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
                        const visibleRows = grupo.querySelectorAll('.permiso-row[style="display: none;"]').length;
                        const totalRows = grupo.querySelectorAll('.permiso-row').length;

                        if (searchValue && totalRows - visibleRows > 0) {
                            // Expandir el grupo si hay resultados y hay texto en la búsqueda
                            bootstrap.Collapse.getOrCreateInstance(grupo).show();
                        }
                    });
                });

                clearSearchBtn?.addEventListener('click', function () {
                    searchInput.value = '';
                    searchInput.dispatchEvent(new Event('input'));

                    // Colapsar todos los grupos
                    document.querySelectorAll('.accordion-collapse.show').forEach(grupo => {
                        bootstrap.Collapse.getOrCreateInstance(grupo).hide();
                    });
                });
            }

            // Toggle estado permiso
            this.setupEstadoToggle('.toggle-estado', 'estadoModal', 'confirmEstadoBtn',
                '/Permisos/ToggleEstado/', 'permiso');
        },

        // Manejadores para Roles
        setupRolesHandlers: function () {
            // Abrir el primer grupo de permisos
            document.querySelector('.accordion-button')?.click();

            // Botones para seleccionar/deseleccionar todos
            document.querySelectorAll('.btn-seleccionar-todos').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    const container = btn.closest('.accordion-body');
                    container.querySelectorAll('input[type="checkbox"]').forEach(cb => {
                        cb.checked = true;
                    });
                });
            });

            document.querySelectorAll('.btn-deseleccionar-todos').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    const container = btn.closest('.accordion-body');
                    container.querySelectorAll('input[type="checkbox"]').forEach(cb => {
                        cb.checked = false;
                    });
                });
            });

            // Filtrado de permisos en vista de detalle
            const filterInput = document.getElementById('permisosFilter');
            if (filterInput) {
                filterInput.addEventListener('keyup', function () {
                    const value = this.value.toLowerCase();

                    document.querySelectorAll('.permiso-row').forEach(row => {
                        const text = row.textContent.toLowerCase();
                        row.style.display = text.includes(value) ? '' : 'none';
                    });
                });
            }
        },

        // Configurar toggle de estado (compartido entre usuarios y permisos)
        setupEstadoToggle: function (toggleSelector, modalId, confirmBtnId, urlBase, entityName) {
            // Token para solicitudes AJAX
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Botones de cambio de estado
            document.querySelectorAll(toggleSelector).forEach(btn => {
                btn.addEventListener('click', function () {
                    if (this.disabled) return;

                    const id = this.dataset.id;
                    const estado = this.dataset.estado === 'true';
                    const entityLabel = this.closest('tr').querySelector('td:first-child').textContent;

                    // Configurar el modal
                    document.getElementById('estadoModalMessage').textContent =
                        `¿Está seguro que desea ${estado ? 'desactivar' : 'activar'} ${entityName === 'usuario' ? 'al' : 'el'} ${entityName} "${entityLabel}"?`;

                    // Configurar botón de confirmación
                    const confirmBtn = document.getElementById(confirmBtnId);
                    confirmBtn.onclick = function () {
                        // Enviar solicitud AJAX
                        fetch(`${urlBase}${id}`, {
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
                                    bootstrap.Modal.getInstance(document.getElementById(modalId)).hide();

                                    // Mostrar notificación
                                    const alert = document.createElement('div');
                                    alert.className = 'alert alert-success alert-dismissible fade show';
                                    alert.innerHTML = `<i class="bi bi-check-circle me-2"></i> ${data.message}
                                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button>`;
                                    document.querySelector('.container-fluid').prepend(alert);

                                    // Actualizar UI
                                    const row = document.querySelector(`[data-id="${id}"]`).closest('tr');
                                    const estadoCell = row.querySelector('td:nth-child(4)');
                                    const toggleBtn = row.querySelector(toggleSelector);

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
                                    App.notify.error(data.message);
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                                App.notify.error(`Ha ocurrido un error al cambiar el estado del ${entityName}`);
                            });
                    };

                    // Mostrar modal
                    const modal = new bootstrap.Modal(document.getElementById(modalId));
                    modal.show();
                });
            });
        },

        // Generador de código a partir del nombre
        setupCodigoGenerator: function () {
            const nombreInput = document.getElementById('Nombre');
            const codigoInput = document.getElementById('Codigo');
            const grupoSelect = document.getElementById('Grupo');

            // Solo si es formulario de creación y los campos no son de solo lectura
            if (nombreInput && codigoInput && !nombreInput.readOnly && !codigoInput.readOnly) {
                nombreInput.addEventListener('input', function () {
                    // Solo generar si el código está vacío o el usuario no lo ha modificado manualmente
                    if (codigoInput.value === '' || codigoInput._lastGenerated) {
                        const grupo = grupoSelect.value.toLowerCase();
                        const accion = nombreInput.value.toLowerCase()
                            .replace(/^(ver|crear|editar|eliminar|autorizar|rechazar|ajustar)\s+/i, '') // Quitar verbo al inicio si existe
                            .trim()
                            .replace(/\s+/g, '.'); // Reemplazar espacios por puntos

                        // Determinar el verbo basado en el nombre
                        let verbo = '';
                        if (/^ver\s+/i.test(nombreInput.value)) verbo = 'ver';
                        else if (/^crear\s+/i.test(nombreInput.value)) verbo = 'crear';
                        else if (/^editar\s+/i.test(nombreInput.value)) verbo = 'editar';
                        else if (/^eliminar\s+/i.test(nombreInput.value)) verbo = 'eliminar';
                        else if (/^autorizar\s+/i.test(nombreInput.value)) verbo = 'autorizar';
                        else if (/^rechazar\s+/i.test(nombreInput.value)) verbo = 'rechazar';
                        else if (/^ajustar\s+/i.test(nombreInput.value)) verbo = 'ajustar';

                        // Generar código
                        if (grupo === 'general') {
                            codigoInput.value = accion.toLowerCase();
                        } else {
                            // Convertir grupo a singular si es posible
                            let grupoSingular = App.usuarioPermisoController.pluralToSingular(grupo);

                            if (verbo) {
                                codigoInput.value = grupoSingular.toLowerCase() + '.' + verbo;
                            } else {
                                codigoInput.value = grupoSingular.toLowerCase() + '.' + accion.toLowerCase();
                            }
                        }

                        // Marcar que el código fue generado automáticamente
                        codigoInput._lastGenerated = true;
                    }
                });

                // Cuando el usuario modifica manualmente el código, dejar de generarlo automáticamente
                codigoInput.addEventListener('input', function () {
                    codigoInput._lastGenerated = false;
                });

                // Cuando cambia el grupo, actualizar el código si se estaba generando automáticamente
                grupoSelect.addEventListener('change', function () {
                    if (codigoInput._lastGenerated) {
                        // Simular un cambio en el nombre para regenerar el código
                        const event = new Event('input', { bubbles: true });
                        nombreInput.dispatchEvent(event);
                    }
                });
            }
        },

        // Convertir plural a singular
        pluralToSingular: function (word) {
            const plurals = {
                'usuarios': 'usuario',
                'roles': 'rol',
                'permisos': 'permiso',
                'ventas': 'venta',
                'productos': 'producto',
                'clientes': 'cliente',
                'reportes': 'reporte',
                'configuración': 'configuracion',
                'proveedores': 'proveedor',
                'cotizaciones': 'cotizacion'
            };

            return plurals[word] || word;
        }
    };

    // Para mantener compatibilidad con código existente
    App.usuariosPermisos = App.usuarioPermisoController;
    App.permisoForm = {
        init: function () {
            App.usuarioPermisoController.setupCodigoGenerator();
        }
    };
    App.auth = {
        init: function () {
            App.validation.setupPasswordStrength('#ContraseñaNueva, #inputPassword', '#password-strength-feedback');
        },
        setupPasswordValidation: function () {
            App.validation.setupPasswordStrength('#ContraseñaNueva, #inputPassword', '#password-strength-feedback');
        }
    };

})(window, jQuery);