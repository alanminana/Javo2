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
    // Agregar filtro de autorización global
    options.Filters.Add(new AuthorizeFilter());
});

// Registro de servicios de autenticación
builder.Services.AddAuthenticationServices();

// Registro de servicios de la aplicación
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
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); 
builder.Services.AddPermissionPolicies();

// Devoluciones debe ser Scoped para poder inyectar IVentaService, IProductoService, etc.
builder.Services.AddScoped<IDevolucionGarantiaService, DevolucionGarantiaService>();

// Servicio de búsqueda de cliente
builder.Services.AddScoped<IClienteSearchService>(sp =>
    sp.GetRequiredService<IClienteService>() as IClienteSearchService
    ?? throw new InvalidOperationException("ClienteService must implement IClienteSearchService"));

// Otros servicios Scoped
builder.Services.AddScoped<IDropdownService, DropdownService>();

// Configuración de AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Validación de la configuración de AutoMapper
try
{
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}
catch (AutoMapper.AutoMapperConfigurationException ex)
{
    Console.WriteLine("Errores de configuración de AutoMapper:");
    Console.WriteLine(ex.Message);
    foreach (var failure in ex.Errors)
    {
        Console.WriteLine($"- {failure}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Middleware para encabezados de seguridad
app.Use(async (context, next) =>
{
    // Agregar encabezados de seguridad con configuración más permisiva
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    // Política CSP más permisiva
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://code.jquery.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self' ws: wss: http: https:;");

    await next();
});

// Middleware para manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configurar rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Crear datos iniciales en desarrollo
if (app.Environment.IsDevelopment())
{
    // Asegurarse de que el directorio Data existe
    var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
    if (!Directory.Exists(dataDir))
    {
        Directory.CreateDirectory(dataDir);
    }

    // Asegurarse de que los archivos JSON existen
    string[] jsonFiles = {
        "usuarios.json",
        "roles.json",
        "permisos.json",
        "configuracion.json",
        "passwordResetTokens.json"
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

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var usuarioService = services.GetRequiredService<IUsuarioService>();
            var rolService = services.GetRequiredService<IRolService>();
            var permisoService = services.GetRequiredService<IPermisoService>();

            // Verificar si hay usuarios
            var usuarios = await usuarioService.GetAllUsuariosAsync();
            if (!usuarios.Any())
            {
                // Crear rol administrador
                var adminRol = new Rol
                {
                    Nombre = "Administrador",
                    Descripcion = "Acceso completo al sistema",
                    EsSistema = true
                };
                var rolId = await rolService.CreateRolAsync(adminRol);

                // Crear usuario administrador
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

                // Asignar rol
                await usuarioService.AsignarRolAsync(admin.UsuarioID, rolId);

                app.Logger.LogInformation("Datos de prueba creados exitosamente");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error al crear datos de prueba: {Message}", ex.Message);
        }
    }
}

app.Run();