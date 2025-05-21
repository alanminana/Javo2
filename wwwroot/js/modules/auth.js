// auth.js - Módulo unificado para autenticación, usuarios y permisos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.auth = {
        // Caché para permisos verificados
        permissionsCache: {},

        // INICIALIZACIÓN
        init: function () {
            console.log('Inicializando módulo unificado de autenticación y permisos');

            // Detectar contexto
            const isLoginForm = $('#loginForm, #resetPasswordForm').length > 0;
            const isUsuariosView = $('#usuariosFilterForm, #estadoModal').length > 0;
            const isRolesView = $('.btn-seleccionar-todos').length > 0;
            const isPermisosView = $('#searchPermiso').length > 0;

            // Inicializar componentes según contexto
            this.setupPasswordValidation();
            this.setupPermissionElements();

            if (isUsuariosView || isRolesView || isPermisosView) {
                this.setupUsuariosHandlers();
                this.setupPermisosHandlers();
                this.setupRolesHandlers();
            }
        },

        // AUTENTICACIÓN Y CONTRASEÑAS

        // Configurar validación de contraseñas
        setupPasswordValidation: function () {
            const passwordFields = [
                document.getElementById('ContraseñaNueva'),
                document.getElementById('inputPassword')
            ];

            passwordFields.forEach(field => {
                if (field) {
                    field.addEventListener('input', function () {
                        const valor = this.value;
                        const validaciones = {
                            longitud: valor.length >= 6,
                            letraNumero: /[a-zA-Z]/.test(valor) && /[0-9]/.test(valor),
                            caracterEspecial: /[^a-zA-Z0-9]/.test(valor)
                        };

                        // Buscar o crear el contenedor de feedback
                        let feedbackDiv;
                        if (this.id === 'ContraseñaNueva') {
                            feedbackDiv = document.querySelector('.alert-info ul');

                            if (feedbackDiv) {
                                const elementos = feedbackDiv.querySelectorAll('li');

                                // Verificar longitud mínima
                                if (elementos[0]) {
                                    if (validaciones.longitud) {
                                        elementos[0].classList.add('text-success');
                                        elementos[0].classList.remove('text-light');
                                    } else {
                                        elementos[0].classList.remove('text-success');
                                        elementos[0].classList.add('text-light');
                                    }
                                }

                                // Verificar letras y números
                                if (elementos[1]) {
                                    if (validaciones.letraNumero) {
                                        elementos[1].classList.add('text-success');
                                        elementos[1].classList.remove('text-light');
                                    } else {
                                        elementos[1].classList.remove('text-success');
                                        elementos[1].classList.add('text-light');
                                    }
                                }

                                // Verificar caracteres especiales
                                if (elementos[2]) {
                                    if (validaciones.caracterEspecial) {
                                        elementos[2].classList.add('text-success');
                                        elementos[2].classList.remove('text-light');
                                    } else {
                                        elementos[2].classList.remove('text-success');
                                        elementos[2].classList.add('text-light');
                                    }
                                }
                            }
                        } else {
                            // Para resetear contraseña
                            feedbackDiv = document.getElementById('password-strength-feedback');
                            if (!feedbackDiv) {
                                feedbackDiv = document.createElement('div');
                                feedbackDiv.id = 'password-strength-feedback';
                                feedbackDiv.className = 'mt-2 small';
                                this.parentNode.appendChild(feedbackDiv);
                            }

                            // Crear contenido de retroalimentación
                            let html = '<div class="fw-bold mb-1">Fortaleza de la contraseña:</div>';
                            html += '<ul class="mb-0 ps-3">';
                            html += `<li class="${validaciones.longitud ? 'text-success' : 'text-danger'}">Al menos 6 caracteres</li>`;
                            html += `<li class="${validaciones.letraNumero ? 'text-success' : 'text-danger'}">Letras y números</li>`;
                            html += `<li class="${validaciones.caracterEspecial ? 'text-success' : 'text-danger'}">Al menos un carácter especial</li>`;
                            html += '</ul>';

                            feedbackDiv.innerHTML = html;
                        }
                    });
                }
            });
        },

        // Validar contraseña
        validatePassword: function (password) {
            if (!password) return false;

            const validations = {
                length: password.length >= 6,
                letterAndNumber: /[a-zA-Z]/.test(password) && /[0-9]/.test(password),
                specialChar: /[^a-zA-Z0-9]/.test(password)
            };

            return validations.length && validations.letterAndNumber && validations.specialChar;
        },

        // Comprobar coincidencia de contraseñas
        checkPasswordMatch: function (password, confirmPassword) {
            return password === confirmPassword;
        },

        // Generar contraseña segura
        generateSecurePassword: function (length = 12) {
            const lowercase = 'abcdefghijklmnopqrstuvwxyz';
            const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            const numbers = '0123456789';
            const special = '!@#$%^&*()_+~`|}{[]\\:;?><,./-=';

            const allChars = lowercase + uppercase + numbers + special;
            let password = '';

            // Asegurar al menos un carácter de cada tipo
            password += lowercase.charAt(Math.floor(Math.random() * lowercase.length));
            password += uppercase.charAt(Math.floor(Math.random() * uppercase.length));
            password += numbers.charAt(Math.floor(Math.random() * numbers.length));
            password += special.charAt(Math.floor(Math.random() * special.length));

            // Completar con caracteres aleatorios
            for (let i = 4; i < length; i++) {
                password += allChars.charAt(Math.floor(Math.random() * allChars.length));
            }

            // Mezclar los caracteres
            return password.split('').sort(() => 0.5 - Math.random()).join('');
        },

        // GESTIÓN DE PERMISOS

        // Configurar elementos basados en permisos
        setupPermissionElements: function () {
            // Ocultar elementos que requieren permisos
            document.querySelectorAll('[data-require-permission]').forEach(element => {
                const permission = element.getAttribute('data-require-permission');
                if (!this.hasPermission(permission)) {
                    element.style.display = 'none';
                }
            });

            // Deshabilitar botones que requieren permisos
            document.querySelectorAll('button[data-require-permission]').forEach(button => {
                const permission = button.getAttribute('data-require-permission');
                if (!this.hasPermission(permission)) {
                    button.disabled = true;
                    button.classList.add('disabled');
                }
            });

            // Quitar eventos de elementos sin permiso
            document.querySelectorAll('[data-require-permission]').forEach(element => {
                const permission = element.getAttribute('data-require-permission');
                if (!this.hasPermission(permission)) {
                    // Clonar y reemplazar para eliminar eventos
                    const clone = element.cloneNode(true);
                    element.parentNode.replaceChild(clone, element);
                }
            });
        },

        // Verificar si el usuario tiene un permiso
        hasPermission: function (permissionCode) {
            if (!permissionCode) return true;

            if (this.permissionsCache[permissionCode] !== undefined) {
                return this.permissionsCache[permissionCode];
            }

            // Intentar obtener del elemento data
            const permData = document.getElementById('userPermissions');
            if (permData && permData.dataset.permissions) {
                try {
                    const permissions = JSON.parse(permData.dataset.permissions);
                    const result = permissions.includes(permissionCode);
                    this.permissionsCache[permissionCode] = result;
                    return result;
                } catch (e) {
                    console.error('Error al parsear permisos:', e);
                }
            }

            // Fallback: buscar en elementos específicos
            const hasDirectPermission = document.querySelector(`[data-permission-code="${permissionCode}"]`) !== null;
            this.permissionsCache[permissionCode] = hasDirectPermission;
            return hasDirectPermission;
        },

        // Verificar múltiples permisos (requiere todos)
        hasAllPermissions: function (permissionCodes) {
            if (!permissionCodes || !permissionCodes.length) return true;

            return permissionCodes.every(code => this.hasPermission(code));
        },

        // Verificar si tiene alguno de los permisos
        hasAnyPermission: function (permissionCodes) {
            if (!permissionCodes || !permissionCodes.length) return true;

            return permissionCodes.some(code => this.hasPermission(code));
        },

        // GESTIÓN DE USUARIOS Y PERMISOS

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

            // Inicializar formularios de usuario
            this.setupUserForms();
        },

        // Configurar formularios de usuario
        setupUserForms: function () {
            // Validación de formulario de usuario
            const userForm = document.getElementById('userForm');
            if (userForm) {
                userForm.addEventListener('submit', function (e) {
                    let isValid = true;

                    // Validar campos requeridos
                    userForm.querySelectorAll('[required]').forEach(field => {
                        if (!field.value.trim()) {
                            isValid = false;
                            field.classList.add('is-invalid');

                            // Crear mensaje de error si no existe
                            let errorDiv = field.nextElementSibling;
                            if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                                errorDiv = document.createElement('div');
                                errorDiv.className = 'invalid-feedback';
                                errorDiv.textContent = 'Este campo es obligatorio';
                                field.parentNode.insertBefore(errorDiv, field.nextSibling);
                            }
                        } else {
                            field.classList.remove('is-invalid');
                        }
                    });

                    // Validar email
                    const emailField = userForm.querySelector('#Email');
                    if (emailField && emailField.value) {
                        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                        if (!emailRegex.test(emailField.value)) {
                            isValid = false;
                            emailField.classList.add('is-invalid');

                            let errorDiv = emailField.nextElementSibling;
                            if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                                errorDiv = document.createElement('div');
                                errorDiv.className = 'invalid-feedback';
                                errorDiv.textContent = 'Ingrese un email válido';
                                emailField.parentNode.insertBefore(errorDiv, emailField.nextSibling);
                            } else {
                                errorDiv.textContent = 'Ingrese un email válido';
                            }
                        }
                    }

                    // Validar contraseña si está presente
                    const passwordField = userForm.querySelector('#Password');
                    const confirmPasswordField = userForm.querySelector('#ConfirmPassword');

                    if (passwordField && confirmPasswordField) {
                        // Si se está creando un usuario o se está cambiando la contraseña
                        if (passwordField.value || confirmPasswordField.value) {
                            if (!App.auth.validatePassword(passwordField.value)) {
                                isValid = false;
                                passwordField.classList.add('is-invalid');

                                let errorDiv = passwordField.nextElementSibling;
                                if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                                    errorDiv = document.createElement('div');
                                    errorDiv.className = 'invalid-feedback';
                                    errorDiv.textContent = 'La contraseña no cumple los requisitos de seguridad';
                                    passwordField.parentNode.insertBefore(errorDiv, passwordField.nextSibling);
                                }
                            }

                            if (passwordField.value !== confirmPasswordField.value) {
                                isValid = false;
                                confirmPasswordField.classList.add('is-invalid');

                                let errorDiv = confirmPasswordField.nextElementSibling;
                                if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                                    errorDiv = document.createElement('div');
                                    errorDiv.className = 'invalid-feedback';
                                    errorDiv.textContent = 'Las contraseñas no coinciden';
                                    confirmPasswordField.parentNode.insertBefore(errorDiv, confirmPasswordField.nextSibling);
                                }
                            }
                        }
                    }

                    if (!isValid) {
                        e.preventDefault();
                        e.stopPropagation();

                        // Scroll al primer error
                        const firstErrorField = userForm.querySelector('.is-invalid');
                        if (firstErrorField) {
                            firstErrorField.scrollIntoView({ behavior: 'smooth', block: 'center' });
                            firstErrorField.focus();
                        }
                    }
                });

                // Generar contraseña segura
                const generatePasswordBtn = document.getElementById('generatePassword');
                if (generatePasswordBtn) {
                    generatePasswordBtn.addEventListener('click', function () {
                        const password = App.auth.generateSecurePassword();
                        document.getElementById('Password').value = password;
                        document.getElementById('ConfirmPassword').value = password;

                        // Mostrar la contraseña generada
                        const passwordDisplayEl = document.getElementById('generatedPasswordDisplay');
                        if (passwordDisplayEl) {
                            passwordDisplayEl.textContent = password;
                            passwordDisplayEl.parentElement.classList.remove('d-none');
                        }

                        // Disparar evento input para validación
                        document.getElementById('Password').dispatchEvent(new Event('input'));
                    });
                }
            }
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
                        const totalRows = grupo.querySelectorAll('.permiso-row').length;
                        const hiddenRows = grupo.querySelectorAll('.permiso-row[style="display: none;"]').length;
                        const visibleRows = totalRows - hiddenRows;

                        if (searchValue && visibleRows > 0) {
                            // Expandir el grupo si hay resultados y hay texto en la búsqueda
                            bootstrap.Collapse.getOrCreateInstance(grupo).show();
                        } else if (searchValue && visibleRows === 0) {
                            // Colapsar grupos sin resultados
                            bootstrap.Collapse.getOrCreateInstance(grupo).hide();
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

            // Configurar formularios de permisos
            this.setupPermisoForm();
        },

        // Configurar formulario de permisos
        setupPermisoForm: function () {
            // Generador de código a partir del nombre
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
                            let grupoSingular = App.auth.pluralToSingular(grupo);

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

            // Validación del formulario de rol
            const rolForm = document.getElementById('rolForm');
            if (rolForm) {
                rolForm.addEventListener('submit', function (e) {
                    let isValid = true;

                    // Validar nombre del rol
                    const nombreField = document.getElementById('Nombre');
                    if (!nombreField.value.trim()) {
                        isValid = false;
                        nombreField.classList.add('is-invalid');

                        // Crear mensaje de error si no existe
                        if (!nombreField.nextElementSibling || !nombreField.nextElementSibling.classList.contains('invalid-feedback')) {
                            const errorDiv = document.createElement('div');
                            errorDiv.className = 'invalid-feedback';
                            errorDiv.textContent = 'El nombre del rol es obligatorio';
                            nombreField.parentNode.insertBefore(errorDiv, nombreField.nextSibling);
                        }
                    } else {
                        nombreField.classList.remove('is-invalid');
                    }

                    // Validar que al menos un permiso esté seleccionado
                    const permisosSeleccionados = rolForm.querySelectorAll('input[type="checkbox"][name^="Permisos"]:checked');
                    const errorContainer = document.getElementById('permisosError');

                    if (permisosSeleccionados.length === 0) {
                        isValid = false;

                        if (errorContainer) {
                            errorContainer.textContent = 'Debe seleccionar al menos un permiso';
                            errorContainer.classList.remove('d-none');
                        } else {
                            // Crear contenedor de error si no existe
                            const newErrorContainer = document.createElement('div');
                            newErrorContainer.id = 'permisosError';
                            newErrorContainer.className = 'alert alert-danger mt-3';
                            newErrorContainer.textContent = 'Debe seleccionar al menos un permiso';

                            const permisosContainer = document.querySelector('.accordion');
                            if (permisosContainer) {
                                permisosContainer.parentNode.insertBefore(newErrorContainer, permisosContainer.nextSibling);
                            }
                        }
                    } else if (errorContainer) {
                        errorContainer.classList.add('d-none');
                    }

                    if (!isValid) {
                        e.preventDefault();
                        e.stopPropagation();

                        // Scroll al primer error
                        const firstErrorField = document.querySelector('.is-invalid') || errorContainer;
                        if (firstErrorField) {
                            firstErrorField.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        }
                    }
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
                                    if (App.notify) {
                                        App.notify.error(data.message);
                                    } else {
                                        alert(data.message);
                                    }
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                                if (App.notify) {
                                    App.notify.error(`Ha ocurrido un error al cambiar el estado del ${entityName}`);
                                } else {
                                    alert(`Ha ocurrido un error al cambiar el estado del ${entityName}`);
                                }
                            });
                    };

                    // Mostrar modal
                    const modal = new bootstrap.Modal(document.getElementById(modalId));
                    modal.show();
                });
            });
        },

        // UTILIDADES

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

    // Alias para compatibilidad con código existente
    App.permissions = {
        cache: App.auth.permissionsCache,
        init: function () {
            App.auth.setupPermissionElements();
        },
        setupPermissionElements: App.auth.setupPermissionElements,
        hasPermission: function (permissionCode) {
            return App.auth.hasPermission(permissionCode);
        }
    };
    App.usuariosPermisos = {
        init: function () {
            App.auth.setupUsuariosHandlers();
            App.auth.setupPermisosHandlers();
            App.auth.setupRolesHandlers();
        },
        setupUsuariosHandlers: App.auth.setupUsuariosHandlers,
        setupPermisosHandlers: App.auth.setupPermisosHandlers,
        setupRolesHandlers: App.auth.setupRolesHandlers,
        setupEstadoToggle: App.auth.setupEstadoToggle
    };

    App.permisoForm = {
        init: function () {
            App.auth.setupPermisoForm();
        },
        setupCodigoGenerator: App.auth.setupPermisoForm
    };

})(window, jQuery);