// Data/Seeders/PermissionSeeder.cs
using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Data.Seeders
{
    public class PermissionSeeder
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;
        private readonly ILogger<PermissionSeeder> _logger;

        public PermissionSeeder(
            IPermisoService permisoService,
            IRolService rolService,
            ILogger<PermissionSeeder> logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Lista completa de todos los permisos del sistema
                var permisosNecesarios = new List<(string Codigo, string Nombre, string Grupo, string Descripcion)>
                {
                    // Permisos de usuario
                    ("usuarios.ver", "Ver usuarios", "Usuarios", "Permite ver la lista de usuarios"),
                    ("usuarios.crear", "Crear usuarios", "Usuarios", "Permite crear nuevos usuarios"),
                    ("usuarios.editar", "Editar usuarios", "Usuarios", "Permite modificar usuarios existentes"),
                    ("usuarios.eliminar", "Eliminar usuarios", "Usuarios", "Permite eliminar usuarios"),
                    
                    // Permisos de roles
                    ("roles.ver", "Ver roles", "Roles", "Permite ver la lista de roles"),
                    ("roles.crear", "Crear roles", "Roles", "Permite crear nuevos roles"),
                    ("roles.editar", "Editar roles", "Roles", "Permite modificar roles existentes"),
                    ("roles.eliminar", "Eliminar roles", "Roles", "Permite eliminar roles"),
                    
                    // Permisos de permisos
                    ("permisos.ver", "Ver permisos", "Permisos", "Permite ver la lista de permisos"),
                    ("permisos.crear", "Crear permisos", "Permisos", "Permite crear nuevos permisos"),
                    ("permisos.editar", "Editar permisos", "Permisos", "Permite modificar permisos existentes"),
                    ("permisos.eliminar", "Eliminar permisos", "Permisos", "Permite eliminar permisos"),
                    
                    // Permisos de ventas
                    ("ventas.ver", "Ver ventas", "Ventas", "Permite ver la lista de ventas"),
                    ("ventas.crear", "Crear ventas", "Ventas", "Permite crear nuevas ventas"),
                    ("ventas.editar", "Editar ventas", "Ventas", "Permite modificar ventas existentes"),
                    ("ventas.eliminar", "Eliminar ventas", "Ventas", "Permite eliminar ventas"),
                    ("ventas.autorizar", "Autorizar ventas", "Ventas", "Permite autorizar ventas"),
                    ("ventas.rechazar", "Rechazar ventas", "Ventas", "Permite rechazar ventas"),
                    ("ventas.entregaProductos", "Entregar productos", "Ventas", "Permite gestionar la entrega de productos"),
                    
                    // Permisos de productos
                    ("productos.ver", "Ver productos", "Productos", "Permite ver la lista de productos"),
                    ("productos.crear", "Crear productos", "Productos", "Permite crear nuevos productos"),
                    ("productos.editar", "Editar productos", "Productos", "Permite modificar productos existentes"),
                    ("productos.eliminar", "Eliminar productos", "Productos", "Permite eliminar productos"),
                    ("productos.ajustarprecios", "Ajustar precios", "Productos", "Permite ajustar precios de productos"),
                    
                    // Permisos de clientes
                    ("clientes.ver", "Ver clientes", "Clientes", "Permite ver la lista de clientes"),
                    ("clientes.crear", "Crear clientes", "Clientes", "Permite crear nuevos clientes"),
                    ("clientes.editar", "Editar clientes", "Clientes", "Permite modificar clientes existentes"),
                    ("clientes.eliminar", "Eliminar clientes", "Clientes", "Permite eliminar clientes"),
                    
                    // Permisos de reportes
                    ("reportes.ver", "Ver reportes", "Reportes", "Permite ver reportes"),
                    ("reportes.exportar", "Exportar reportes", "Reportes", "Permite exportar reportes"),
                    
                    // Permisos de configuración
                    ("configuracion.ver", "Ver configuración", "Configuración", "Permite ver la configuración del sistema"),
                    ("configuracion.editar", "Editar configuración", "Configuración", "Permite modificar la configuración del sistema"),
                    
                    // Permisos de perfil
                    ("perfil.ver", "Ver perfil", "Perfil", "Permite ver el perfil de usuario"),
                    ("perfil.editar", "Editar perfil", "Perfil", "Permite modificar el perfil de usuario"),
                    
                    // Permisos de catálogo
                    ("catalogo.ver", "Ver catálogo", "Catálogo", "Permite ver el catálogo"),
                    ("catalogo.crear", "Crear en catálogo", "Catálogo", "Permite crear elementos en el catálogo"),
                    ("catalogo.editar", "Editar catálogo", "Catálogo", "Permite modificar elementos del catálogo"),
                    ("catalogo.eliminar", "Eliminar del catálogo", "Catálogo", "Permite eliminar elementos del catálogo"),
                    
                    // Permisos de proveedores
                    ("proveedores.ver", "Ver proveedores", "Proveedores", "Permite ver la lista de proveedores"),
                    ("proveedores.crear", "Crear proveedores", "Proveedores", "Permite crear nuevos proveedores"),
                    ("proveedores.editar", "Editar proveedores", "Proveedores", "Permite modificar proveedores existentes"),
                    ("proveedores.eliminar", "Eliminar proveedores", "Proveedores", "Permite eliminar proveedores"),
                    
                    // Permisos de promociones
                    ("promociones.ver", "Ver promociones", "Promociones", "Permite ver la lista de promociones"),
                    ("promociones.crear", "Crear promociones", "Promociones", "Permite crear nuevas promociones"),
                    ("promociones.editar", "Editar promociones", "Promociones", "Permite modificar promociones existentes"),
                    ("promociones.eliminar", "Eliminar promociones", "Promociones", "Permite eliminar promociones"),
                    
                    // Permisos de devolución/garantía
                    ("devoluciongarantia.ver", "Ver devolución/garantía", "DevolucionGarantia", "Permite ver devoluciones y garantías"),
                    ("devoluciongarantia.crear", "Crear devolución/garantía", "DevolucionGarantia", "Permite crear devoluciones y garantías"),
                    ("devoluciongarantia.editar", "Editar devolución/garantía", "DevolucionGarantia", "Permite modificar devoluciones y garantías"),
                    ("devoluciongarantia.eliminar", "Eliminar devolución/garantía", "DevolucionGarantia", "Permite eliminar devoluciones y garantías"),
                    
                    // Permisos de SecurityDashboard
                    ("securitydashboard.ver", "Ver Dashboard de Seguridad", "Seguridad", "Permite ver el panel de control de seguridad"),
                    ("securitydashboard.crear", "Crear elementos en Dashboard", "Seguridad", "Permite crear elementos en el panel de seguridad"),
                    ("securitydashboard.editar", "Editar elementos en Dashboard", "Seguridad", "Permite editar elementos en el panel de seguridad"),
                    ("securitydashboard.eliminar", "Eliminar elementos en Dashboard", "Seguridad", "Permite eliminar elementos en el panel de seguridad"),
                    
                    // Permisos de auditoria
                    ("auditoria.ver", "Ver auditoría", "Auditoría", "Permite ver registros de auditoría"),
                    ("auditoria.rollback", "Revertir cambios", "Auditoría", "Permite revertir cambios desde la auditoría"),

                    ("productos.ajustarhistorial", "Ver historial de ajustes", "Productos", "Permite ver el historial de ajustes de precios"),


                // En algún archivo de inicialización, como Data/Seeders/PermissionSeeder.cs
                // Agregar estos nuevos permisos:
                ("proveedores.realizarcompra", "Realizar compras a proveedores", "Proveedores", "Permite realizar compras a proveedores"),
("proveedores.vercompras", "Ver compras a proveedores", "Proveedores", "Permite ver las compras realizadas a proveedores")
    };
                // Permisos actuales
                var permisosActuales = await _permisoService.GetAllPermisosAsync();
                var codigosExistentes = permisosActuales.Select(p => p.Codigo).ToList();

                _logger.LogInformation("Verificando {Count} permisos necesarios", permisosNecesarios.Count);

                // Crear permisos que no existen
                foreach (var perm in permisosNecesarios)
                {
                    if (!codigosExistentes.Contains(perm.Codigo))
                    {
                        _logger.LogInformation("Creando permiso: {Codigo}", perm.Codigo);

                        var nuevoPermiso = new Permiso
                        {
                            Codigo = perm.Codigo,
                            Nombre = perm.Nombre,
                            Grupo = perm.Grupo,
                            Descripcion = perm.Descripcion,
                            Activo = true,
                            EsSistema = true
                        };

                        await _permisoService.CreatePermisoAsync(nuevoPermiso);
                        _logger.LogInformation("Permiso creado: {Codigo}", perm.Codigo);
                    }
                }

                // Asegurar que los permisos existentes estén activos
                foreach (var permiso in permisosActuales)
                {
                    if (!permiso.Activo && permisosNecesarios.Any(p => p.Codigo == permiso.Codigo))
                    {
                        _logger.LogInformation("Activando permiso inactivo: {Codigo}", permiso.Codigo);
                        permiso.Activo = true;
                        await _permisoService.UpdatePermisoAsync(permiso);
                    }
                }

                // Asegurarse de que el rol Administrador tenga todos los permisos
                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin != null)
                {
                    _logger.LogInformation("Verificando permisos para el rol Administrador (ID: {RolID})", rolAdmin.RolID);

                    // Obtener todos los permisos actualizados
                    permisosActuales = await _permisoService.GetAllPermisosAsync();
                    var permisosAdmin = rolAdmin.Permisos?.Select(p => p.PermisoID).ToList() ?? new List<int>();

                    foreach (var permiso in permisosActuales)
                    {
                        if (!permisosAdmin.Contains(permiso.PermisoID))
                        {
                            _logger.LogInformation("Asignando permiso {Codigo} (ID: {PermisoID}) al rol Administrador",
                                permiso.Codigo, permiso.PermisoID);

                            try
                            {
                                await _rolService.AsignarPermisoAsync(rolAdmin.RolID, permiso.PermisoID);
                                _logger.LogInformation("Permiso {Codigo} asignado al rol Administrador exitosamente", permiso.Codigo);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al asignar permiso {Codigo} (ID: {PermisoID}) al rol Administrador",
                                    permiso.Codigo, permiso.PermisoID);
                            }
                        }
                    }

                    _logger.LogInformation("Verificación de permisos para el rol Administrador completada");
                }
                else
                {
                    _logger.LogWarning("No se encontró el rol Administrador en el sistema");
                }

                _logger.LogInformation("Proceso de seed de permisos completado con éxito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sembrar permisos");
                throw;
            }
        }
    }
}