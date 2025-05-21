// auth.js - Módulo para autenticación y manejo de contraseñas
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.auth = {
        init: function () {
            this.setupPasswordValidation();
        },

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
        }
    };

})(window, jQuery);