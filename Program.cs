using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapper;
using Javo2;
using Javo2.Extensions;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.IServices.Common;
using Javo2.Middleware;
using Javo2.Services;
using Javo2.Services.Authentication;
using Javo2.Services.Common;
using Javo2.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add<Javo2.Filters.PermissionActionFilter>();
});

// Configuración de autorización consolidada
builder.Services.AddAuthorization(options =>
{
    // Clientes
    options.AddPolicy("Permission:clientes.ver", policy => policy.RequireClaim("Permission", "clientes.ver"));
    options.AddPolicy("Permission:clientes.crear", policy => policy.RequireClaim("Permission", "clientes.crear"));
    options.AddPolicy("Permission:clientes.editar", policy => policy.RequireClaim("Permission", "clientes.editar"));
    options.AddPolicy("Permission:clientes.eliminar", policy => policy.RequireClaim("Permission", "clientes.eliminar"));

    // Módulo
    options.AddPolicy("Permission:modulo.ver", policy => policy.RequireClaim("Permission", "modulo.ver"));
    options.AddPolicy("Permission:modulo.crear", policy => policy.RequireClaim("Permission", "modulo.crear"));
    options.AddPolicy("Permission:modulo.editar", policy => policy.RequireClaim("Permission", "modulo.editar"));
    options.AddPolicy("Permission:modulo.eliminar", policy => policy.RequireClaim("Permission", "modulo.eliminar"));

    // Catálogo
    options.AddPolicy("Permission:catalogo.ver", policy => policy.RequireClaim("Permission", "catalogo.ver"));
    options.AddPolicy("Permission:catalogo.crear", policy => policy.RequireClaim("Permission", "catalogo.crear"));
    options.AddPolicy("Permission:catalogo.editar", policy => policy.RequireClaim("Permission", "catalogo.editar"));
    options.AddPolicy("Permission:catalogo.eliminar", policy => policy.RequireClaim("Permission", "catalogo.eliminar"));

    // Productos
    options.AddPolicy("Permission:productos.ver", policy => policy.RequireClaim("Permission", "productos.ver"));
    options.AddPolicy("Permission:productos.crear", policy => policy.RequireClaim("Permission", "productos.crear"));
    options.AddPolicy("Permission:productos.editar", policy => policy.RequireClaim("Permission", "productos.editar"));
    options.AddPolicy("Permission:productos.eliminar", policy => policy.RequireClaim("Permission", "productos.eliminar"));

    // Ajuste de Precios
    options.AddPolicy("Permission:ajustePrecios.ver", policy => policy.RequireClaim("Permission", "ajustePrecios.ver"));
    options.AddPolicy("Permission:ajustePrecios.crear", policy => policy.RequireClaim("Permission", "ajustePrecios.crear"));

    // Auditoría
    options.AddPolicy("Permission:auditoria.ver", policy => policy.RequireClaim("Permission", "auditoria.ver"));
    options.AddPolicy("Permission:auditoria.rollback", policy => policy.RequireClaim("Permission", "auditoria.rollback"));

    // Configuración
    options.AddPolicy("Permission:configuracion.ver", policy => policy.RequireClaim("Permission", "configuracion.ver"));
    options.AddPolicy("Permission:configuracion.editar", policy => policy.RequireClaim("Permission", "configuracion.editar"));
    options.AddPolicy("Permission:configuracion.seguridad", policy => policy.RequireClaim("Permission", "configuracion.seguridad"));

    // Configuración Inicial
    options.AddPolicy("Permission:configuracionInicial.ver", policy => policy.RequireClaim("Permission", "configuracionInicial.ver"));
    options.AddPolicy("Permission:configuracionInicial.crear", policy => policy.RequireClaim("Permission", "configuracionInicial.crear"));

    // Devolución/Garantía
    options.AddPolicy("Permission:devolucionGarantia.ver", policy => policy.RequireClaim("Permission", "devolucionGarantia.ver"));
    options.AddPolicy("Permission:devolucionGarantia.crear", policy => policy.RequireClaim("Permission", "devolucionGarantia.crear"));
    options.AddPolicy("Permission:devolucionGarantia.editar", policy => policy.RequireClaim("Permission", "devolucionGarantia.editar"));
    options.AddPolicy("Permission:devolucionGarantia.procesar", policy => policy.RequireClaim("Permission", "devolucionGarantia.procesar"));
    options.AddPolicy("Permission:devolucionGarantia.enviarGarantia", policy => policy.RequireClaim("Permission", "devolucionGarantia.enviarGarantia"));
    options.AddPolicy("Permission:devolucionGarantia.completarGarantia", policy => policy.RequireClaim("Permission", "devolucionGarantia.completarGarantia"));
    options.AddPolicy("Permission:devolucionGarantia.eliminar", policy => policy.RequireClaim("Permission", "devolucionGarantia.eliminar"));

    // Diagnostic
    options.AddPolicy("Permission:diagnostic.files", policy => policy.RequireClaim("Permission", "diagnostic.files"));
    options.AddPolicy("Permission:diagnostic.auth", policy => policy.RequireClaim("Permission", "diagnostic.auth"));
    options.AddPolicy("Permission:diagnostic.user", policy => policy.RequireClaim("Permission", "diagnostic.user"));

    // Perfil
    options.AddPolicy("Permission:perfil.ver", policy => policy.RequireClaim("Permission", "perfil.ver"));
    options.AddPolicy("Permission:perfil.editar", policy => policy.RequireClaim("Permission", "perfil.editar"));

    // Permisos
    options.AddPolicy("Permission:permisos.ver", policy => policy.RequireClaim("Permission", "permisos.ver"));
    options.AddPolicy("Permission:permisos.crear", policy => policy.RequireClaim("Permission", "permisos.crear"));
    options.AddPolicy("Permission:permisos.editar", policy => policy.RequireClaim("Permission", "permisos.editar"));
    options.AddPolicy("Permission:permisos.eliminar", policy => policy.RequireClaim("Permission", "permisos.eliminar"));

    // Promociones
    options.AddPolicy("Permission:promociones.ver", policy => policy.RequireClaim("Permission", "promociones.ver"));
    options.AddPolicy("Permission:promociones.crear", policy => policy.RequireClaim("Permission", "promociones.crear"));
    options.AddPolicy("Permission:promociones.editar", policy => policy.RequireClaim("Permission", "promociones.editar"));
    options.AddPolicy("Permission:promociones.eliminar", policy => policy.RequireClaim("Permission", "promociones.eliminar"));

    // Proveedores
    options.AddPolicy("Permission:proveedores.ver", policy => policy.RequireClaim("Permission", "proveedores.ver"));
    options.AddPolicy("Permission:proveedores.crear", policy => policy.RequireClaim("Permission", "proveedores.crear"));
    options.AddPolicy("Permission:proveedores.editar", policy => policy.RequireClaim("Permission", "proveedores.editar"));
    options.AddPolicy("Permission:proveedores.eliminar", policy => policy.RequireClaim("Permission", "proveedores.eliminar"));
    options.AddPolicy("Permission:proveedores.registrarCompra", policy => policy.RequireClaim("Permission", "proveedores.registrarCompra"));

    // Reportes
    options.AddPolicy("Permission:reportes.ver", policy => policy.RequireClaim("Permission", "reportes.ver"));
    options.AddPolicy("Permission:reportes.rankingVentas", policy => policy.RequireClaim("Permission", "reportes.rankingVentas"));
    options.AddPolicy("Permission:reportes.exportVentas", policy => policy.RequireClaim("Permission", "reportes.exportVentas"));
    options.AddPolicy("Permission:reportes.reporteStock", policy => policy.RequireClaim("Permission", "reportes.reporteStock"));

    // Reset Password
    options.AddPolicy("Permission:resetPassword.olvideContraseña", policy => policy.RequireClaim("Permission", "resetPassword.olvideContraseña"));
    options.AddPolicy("Permission:resetPassword.resetearContraseña", policy => policy.RequireClaim("Permission", "resetPassword.resetearContraseña"));

    // Usuarios
    options.AddPolicy("Permission:usuarios.ver", policy => policy.RequireClaim("Permission", "usuarios.ver"));
    options.AddPolicy("Permission:usuarios.crear", policy => policy.RequireClaim("Permission", "usuarios.crear"));
    options.AddPolicy("Permission:usuarios.editar", policy => policy.RequireClaim("Permission", "usuarios.editar"));
    options.AddPolicy("Permission:usuarios.eliminar", policy => policy.RequireClaim("Permission", "usuarios.eliminar"));

    // Ventas
    options.AddPolicy("Permission:ventas.ver", policy => policy.RequireClaim("Permission", "ventas.ver"));
    options.AddPolicy("Permission:ventas.crear", policy => policy.RequireClaim("Permission", "ventas.crear"));
    options.AddPolicy("Permission:ventas.editar", policy => policy.RequireClaim("Permission", "ventas.editar"));
    options.AddPolicy("Permission:ventas.eliminar", policy => policy.RequireClaim("Permission", "ventas.eliminar"));
    options.AddPolicy("Permission:ventas.autorizaciones", policy => policy.RequireClaim("Permission", "ventas.autorizaciones"));
    options.AddPolicy("Permission:ventas.autorizar", policy => policy.RequireClaim("Permission", "ventas.autorizar"));
    options.AddPolicy("Permission:ventas.rechazar", policy => policy.RequireClaim("Permission", "ventas.rechazar"));
    options.AddPolicy("Permission:ventas.entrega", policy => policy.RequireClaim("Permission", "ventas.entrega"));
    options.AddPolicy("Permission:ventas.reimprimir", policy => policy.RequireClaim("Permission", "ventas.reimprimir"));
    options.AddPolicy("Permission:ventas.entregaProductos", policy => policy.RequireClaim("Permission", "ventas.entregaProductos"));
});

// Registro de servicios
builder.Services.AddAuthenticationServices();

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IProvinciaService, ProvinciaService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPromocionesService, PromocionesService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICotizacionService, CotizacionService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddPermissionPolicies();

// Devoluciones
builder.Services.AddScoped<IDevolucionGarantiaService, DevolucionGarantiaService>();

// Servicio de búsqueda de cliente
builder.Services.AddScoped<IClienteSearchService>(sp =>
    sp.GetRequiredService<IClienteService>() as IClienteSearchService
    ?? throw new InvalidOperationException("ClienteService must implement IClienteSearchService"));

builder.Services.AddScoped<IDropdownService, DropdownService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Validación de configuración AutoMapper
try
{
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}
catch (AutoMapperConfigurationException ex)
{
    Console.WriteLine("Errores de configuración de AutoMapper:");
    Console.WriteLine(ex.Message);
    foreach (var failure in ex.Errors)
        Console.WriteLine($"- {failure}");
}

// Middleware estándar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Encabezados de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://code.jquery.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self' ws: wss: http: https:;");
    await next();
});

// Middleware personalizado
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicialización en desarrollo
if (app.Environment.IsDevelopment())
{
    var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
    if (!Directory.Exists(dataDir))
        Directory.CreateDirectory(dataDir);

    string[] jsonFiles = {
        "usuarios.json", "roles.json", "permisos.json",
        "configuracion.json", "passwordResetTokens.json"
    };

    foreach (var file in jsonFiles)
    {
        var filePath = Path.Combine(dataDir, file);
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
            Console.WriteLine($"Archivo creado: {filePath}");
        }
    }

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var usuarioService = services.GetRequiredService<IUsuarioService>();
        var rolService = services.GetRequiredService<IRolService>();
        var permisoService = services.GetRequiredService<IPermisoService>();

        var usuarios = await usuarioService.GetAllUsuariosAsync();
        if (!usuarios.Any())
        {
            var adminRol = new Rol
            {
                Nombre = "Administrador",
                Descripcion = "Acceso completo al sistema",
                EsSistema = true
            };
            var rolId = await rolService.CreateRolAsync(adminRol);

            var admin = new Usuario
            {
                NombreUsuario = "admin",
                Nombre = "Administrador",
                Apellido = "Sistema",
                Email = "admin@sistema.com",
                Activo = true,
                CreadoPor = "Sistema"
            };
            await usuarioService.CreateUsuarioAsync(admin, "Admin123!");
            await usuarioService.AsignarRolAsync(admin.UsuarioID, rolId);

            app.Logger.LogInformation("Datos de prueba creados exitosamente");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al crear datos de prueba: {Message}", ex.Message);
    }
}

app.Run();
