using AutoMapper;
using FluentAssertions.Common;
using Javo2;
using Javo2.Extensions;
using Javo2.Filters;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.IServices.Common;
using Javo2.Middleware;
using Javo2.Services.Authentication;
using Javo2.Services.Catalog;
using Javo2.Services.Client;
using Javo2.Services.Common;
using Javo2.Services.Finance;
using Javo2.Services.inventory;
using Javo2.Services.Operations;
using Javo2.Services.Reporting;
using Javo2.Services.System;
using Javo2.TagHelpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Registrar servicios
builder.Services.AddControllersWithViews(options =>
{
    // elimina el [Required] automático en reference types no-nullable
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}); 
builder.Services.AddHttpContextAccessor();

// Primero registrar servicios básicos
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar autenticación y servicios relacionados
builder.Services.AddAuthenticationServices();  // Registra servicios de autenticación
builder.Services.AddAuthenticationPolicies();  // Registra políticas de permisos
builder.Services.AddScoped<IAjustePrecioService, JsonDataService>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddHostedService<AjustesTemporalesBackgroundService>();

// Servicios de la aplicación
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
builder.Services.AddScoped<IGaranteService, GaranteService>();
builder.Services.AddScoped<IFormCombosService, FormCombosService>();

builder.Services.AddScoped<ICreditoService, CreditoService>();

builder.Services.AddScoped<IDevolucionGarantiaService, DevolucionGarantiaService>();
builder.Services.AddPermissionPolicies();
// Agregar en Program.cs (en la sección de registro de servicios)

// Servicio de garantes
builder.Services.AddScoped<IGaranteService, GaranteService>();

// Asegurarse de que el servicio de cotizaciones esté registrado
builder.Services.AddScoped<ICotizacionService, CotizacionService>();

// Servicio de búsqueda de cliente
builder.Services.AddScoped<IClienteSearchService>(sp =>
    sp.GetRequiredService<IClienteService>() as IClienteSearchService
    ?? throw new InvalidOperationException("ClienteService must implement IClienteSearchService"));

builder.Services.AddScoped<IDropdownService, DropdownService>();

// Registrar TagHelpers y Filters
builder.Services.AddScoped<PermissionTagHelper>();
builder.Services.AddScoped<PermissionSeeder>();
builder.Services.AddMvc(options =>
{
    options.Filters.Add<PermissionSeeder>();
});

// AutoMapper - Forma mejorada
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<AutoMapperProfile>();
});
var app = builder.Build();

// Validación de configuración AutoMapper mejorada
try
{
    var mapper = app.Services.GetService<IMapper>();

    if (mapper != null)
    {
        try
        {
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
        catch (AutoMapperConfigurationException ex)
        {
            Console.WriteLine("⚠️ Advertencia: Configuración incompleta de AutoMapper");
            Console.WriteLine(ex.Message);

            if (ex.Errors != null)
            {
                foreach (var failure in ex.Errors)
                {
                    Console.WriteLine($"- {failure}");
                }
            }
        }
    }
    else
    {
        Console.WriteLine("⚠️ Advertencia: No se pudo obtener una instancia de IMapper");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error al validar AutoMapper: {ex.Message}");
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

// Middleware personalizado
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Autenticación y autorización (antes de endpoints)
app.UseAuthenticationConfig();

// Definir rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Middleware de autenticación personalizado
app.UseMiddleware<AuthenticationMiddleware>();

// Sembrar permisos al inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var permisoService = services.GetRequiredService<IPermisoService>();
    var rolService = services.GetRequiredService<IRolService>();
    var loggerSeeder = services.GetRequiredService<ILogger<Javo2.Data.Seeders.PermissionSeeder>>();

    try
    {
        var seeder = new Javo2.Data.Seeders.PermissionSeeder(permisoService, rolService, loggerSeeder);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sembrar permisos");
    }
}

app.Run();