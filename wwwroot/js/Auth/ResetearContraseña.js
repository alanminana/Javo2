document.addEventListener('DOMContentLoaded', function () {
    const nuevaContraseña = document.getElementById('inputPassword');
    if (nuevaContraseña) {
        nuevaContraseña.addEventListener('input', function () {
            const valor = this.value;
            const validaciones = {
                longitud: valor.length >= 6,
                letraNumero: /[a-zA-Z]/.test(valor) && /[0-9]/.test(valor),
                caracterEspecial: /[^a-zA-Z0-9]/.test(valor)
            };

            // Verificar si ya existe el div de feedback
            let feedbackDiv = document.getElementById('password-strength-feedback');
            if (!feedbackDiv) {
                feedbackDiv = document.createElement('div');
                feedbackDiv.id = 'password-strength-feedback';
                feedbackDiv.className = 'mt-2 small';
                nuevaContraseña.parentNode.appendChild(feedbackDiv);
            }

            // Crear contenido de retroalimentación
            let html = '<div class="fw-bold mb-1">Fortaleza de la contraseña:</div>';
            html += '<ul class="mb-0 ps-3">';
            html += `<li class="${validaciones.longitud ? 'text-success' : 'text-danger'}">Al menos 6 caracteres</li>`;
            html += `<li class="${validaciones.letraNumero ? 'text-success' : 'text-danger'}">Letras y números</li>`;
            html += `<li class="${validaciones.caracterEspecial ? 'text-success' : 'text-danger'}">Al menos un carácter especial</li>`;
            html += '</ul>';

            feedbackDiv.innerHTML = html;
        });
    }
});