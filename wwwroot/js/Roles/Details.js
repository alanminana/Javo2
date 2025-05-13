        // Filtrado de permisos
        document.addEventListener('DOMContentLoaded', function() {
            const filterInput = document.getElementById('permisosFilter');
            if (filterInput) {
                filterInput.addEventListener('keyup', function() {
                    const value = this.value.toLowerCase();

                    document.querySelectorAll('.permiso-row').forEach(row => {
                        const text = row.textContent.toLowerCase();
                        row.style.display = text.includes(value) ? '' : 'none';
                    });
                });
            }
        });
