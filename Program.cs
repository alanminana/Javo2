using System;
using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Javo2;
using Javo2.Extensions;
using Javo2.Middleware;
using Javo2.IServices;
using Javo2.Services;
using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using Javo2.IServices.Common;
using Javo2.Services.Common;
using Javo2.Models.Authentication;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

// — Logging —
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// — MVC + filtro global de autorización —
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// — Autenticación y autorización personalizadas —
builder.Services.AddAuthenticationServices();

// — Servicios de la aplicación —
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
builder.Services.AddScoped<IDevolucionGarantiaService, DevolucionGarantiaService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();
builder.Services.AddScoped<IClienteSearchService>(sp =>
    sp.GetRequiredService<IClienteService>() as IClienteSearchService
        ?? throw new InvalidOperationException("ClienteService debe implementar IClienteSearchService"));

// — AutoMapper —
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// — Validar configuración de AutoMapper —
try
{
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}
catch (AutoMapper.AutoMapperConfigurationException ex)
{
    Console.WriteLine("Errores de AutoMapper:");
    Console.WriteLine(ex.Message);
    foreach (var err in ex.Errors) Console.WriteLine($"- {err}");
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

// — Middlewares de seguridad y excepciones —
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// — Nuestro control de autenticación (redirect/AJAX) —
app.UseMiddleware<AuthenticationMiddleware>();

// — Cookie auth + políticas —
app.UseAuthenticationConfig();

// — Rutas MVC —
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// — Seed de datos en DEV —
if (app.Environment.IsDevelopment())
{
    var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
    if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

    foreach (var file in new[] { "usuarios.json", "roles.json", "permisos.json", "configuracion.json", "passwordResetTokens.json" })
    {
        var path = Path.Combine(dataDir, file);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[]");
            app.Logger.LogInformation($"Creado: {path}");
        }
    }

    using var scope = app.Services.CreateScope();
    var svc = scope.ServiceProvider;
    try
    {
        var usuarioService = svc.GetRequiredService<IUsuarioService>();
        var rolService = svc.GetRequiredService<IRolService>();
        if (!(await usuarioService.GetAllUsuariosAsync()).Any())
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
            app.Logger.LogInformation("Datos de prueba creados");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al seed de datos: {Message}", ex.Message);
    }
}

app.Run();
