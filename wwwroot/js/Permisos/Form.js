
        // Ayuda para generar el código a partir del nombre
        document.addEventListener('DOMContentLoaded', function() {
            const nombreInput = document.getElementById('Nombre');
            const codigoInput = document.getElementById('Codigo');
            const grupoSelect = document.getElementById('Grupo');

            // Solo si es formulario de creación y los campos no son de solo lectura
            if (nombreInput && codigoInput && !nombreInput.readOnly && !codigoInput.readOnly) {
                nombreInput.addEventListener('input', function() {
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
                            let grupoSingular = grupo;
                            if (grupo === 'usuarios') grupoSingular = 'usuario';
                            else if (grupo === 'roles') grupoSingular = 'rol';
                            else if (grupo === 'permisos') grupoSingular = 'permiso';
                            else if (grupo === 'ventas') grupoSingular = 'venta';
                            else if (grupo === 'productos') grupoSingular = 'producto';
                            else if (grupo === 'clientes') grupoSingular = 'cliente';
                            else if (grupo === 'reportes') grupoSingular = 'reporte';
                            else if (grupo === 'configuración') grupoSingular = 'configuracion';

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
                codigoInput.addEventListener('input', function() {
                    codigoInput._lastGenerated = false;
                });

                // Cuando cambia el grupo, actualizar el código si se estaba generando automáticamente
                grupoSelect.addEventListener('change', function() {
                    if (codigoInput._lastGenerated) {
                        // Simular un cambio en el nombre para regenerar el código
                        const event = new Event('input', { bubbles: true });
                        nombreInput.dispatchEvent(event);
                    }
                });
            }
        });
