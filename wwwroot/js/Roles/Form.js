
        document.addEventListener('DOMContentLoaded', function() {
            // Abrir el primer grupo de permisos
            document.querySelector('.accordion-button').click();

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

            // Validación del formulario antes de enviar
            const form = document.querySelector('form');
            form.addEventListener('submit', function(e) {
                console.log('Formulario enviado');

                // Verificar si hay permisos seleccionados
                const permisosSeleccionados = Array.from(document.querySelectorAll('input[name="PermisosSeleccionados"]:checked'))
                    .map(cb => cb.value);
                console.log('Permisos seleccionados:', permisosSeleccionados);

                // No bloqueamos el envío del formulario si no hay permisos seleccionados,
                // ya que puede ser un comportamiento válido
            });
        });
