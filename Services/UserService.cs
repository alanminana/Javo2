using Javo2.Models.Authentication;

private async Task CrearUsuarioAdmin()
{
    try
    {
        var adminExiste = _usuarios.Any(u => u.NombreUsuario.Equals("admin", StringComparison.OrdinalIgnoreCase));

        if (!adminExiste)
        {
            _logger.LogInformation("Creando usuario administrador por defecto");

            var admin = new Usuario
            {
                NombreUsuario = "admin",
                Nombre = "Administrador",
                Apellido = "Sistema",
                Email = "admin@sistema.com",
                Contraseña = HashContraseña("admin"), // Contraseña por defecto: admin
                FechaCreacion = DateTime.Now,
                CreadoPor = "Sistema",
                Activo = true,
                Roles = new List<UsuarioRol>()
            };

            // Asignar ID y agregar a la lista
            lock (_lock)
            {
                admin.UsuarioID = _nextUsuarioID++;
                _usuarios.Add(admin);
                GuardarEnJson();
            }

            // Crear rol administrador si no existe
            var roles = await _rolService.GetAllRolesAsync();
            var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

            if (rolAdmin != null)
            {
                await AsignarRolAsync(admin.UsuarioID, rolAdmin.RolID);
                _logger.LogInformation("Rol Administrador asignado al usuario admin");
            }
            else
            {
                _logger.LogWarning("No se encontró el rol Administrador para asignar al usuario admin");
            }

            _logger.LogInformation("Usuario administrador creado automáticamente");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al crear usuario administrador por defecto");
    }
}