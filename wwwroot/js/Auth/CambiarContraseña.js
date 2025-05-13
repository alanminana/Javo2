// Validación en tiempo real de la fortaleza de la contraseña
document.addEventListener('DOMContentLoaded', function () {
    const nuevaContraseña = document.getElementById('ContraseñaNueva');
    if (nuevaContraseña) {
        nuevaContraseña.addEventListener('input', function () {
            const valor = this.value;
            const elementos = document.querySelectorAll('.alert-info ul li');

            // Verificar longitud mínima
            if (elementos[0]) {
                if (valor.length >= 6) {
                    elementos[0].classList.add('text-success');
                    elementos[0].classList.remove('text-light');
                } else {
                    elementos[0].classList.remove('text-success');
                    elementos[0].classList.add('text-light');
                }
            }

            // Verificar letras y números
            if (elementos[1]) {
                if (/[a-zA-Z]/.test(valor) && /[0-9]/.test(valor)) {
                    elementos[1].classList.add('text-success');
                    elementos[1].classList.remove('text-light');
                } else {
                    elementos[1].classList.remove('text-success');
                    elementos[1].classList.add('text-light');
                }
            }

            // Verificar caracteres especiales
            if (elementos[2]) {
                if (/[^a-zA-Z0-9]/.test(valor)) {
                    elementos[2].classList.add('text-success');
                    elementos[2].classList.remove('text-light');
                } else {
                    elementos[2].classList.remove('text-success');
                    elementos[2].classList.add('text-light');
                }
            }
        });
    }
});