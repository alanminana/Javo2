using AutoMapper;
using Javo2;
using Javo2.Extensions;
using Javo2.Filters;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.IServices.Common;
using Javo2.Middleware;
using Javo2.Services;
using Javo2.Services.Authentication;
using Javo2.Services.Common;
using Javo2.TagHelpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuración de MVC con filtros
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    // ELIMINAR ESTA LÍNEA QUE CAUSA DUPLICACIÓN
    // options.Filters.Add<PermissionSeeder>();
});

builder.Services.AddHttpContextAccessor();

// Servicios básicos
builder.Services.AddScoped<IEmailService, EmailService>();

// Autenticación y políticas
builder.Services.AddAuthenticationServices();
builder.Services.AddAuthenticationPolicies();
builder.Services.AddPermissionPolicies();

// Servicios de dominio - todos como Scoped para evitar problemas de dependencias
builder.Services.AddScoped<IAjustePrecioService, JsonDataService>();
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
builder.Services.AddScoped<IDropdownService, DropdownService>();

// Servicio de búsqueda de cliente (interface compartida)
builder.Services.AddScoped<IClienteSearchService>(sp =>
   sp.GetRequiredService<IClienteService>() as IClienteSearchService
   ?? throw new InvalidOperationException("ClienteService must implement IClienteSearchService"));

// TagHelpers y filtros
builder.Services.AddScoped<PermissionTagHelper>();
builder.Services.AddScoped<PermissionSeeder>();

// Background service
builder.Services.AddHostedService<AjustesTemporalesBackgroundService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

// Añadir soporte SPA para Vue.js
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot/vue/dist";
});

var app = builder.Build();

// Validación de configuración AutoMapper
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
// Agregar middleware SPA
app.UseSpaStaticFiles();
app.UseRouting();

app.UseAuthenticationConfig();

// Rutas
app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Home}/{action=Index}/{id?}");

// Middleware de autenticación personalizada
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

// Configuración SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot/vue";

    // Si estás en desarrollo y quieres usar el servidor de desarrollo de Vue.js
    if (app.Environment.IsDevelopment())
    {
        // Opcional: si tienes configurado npm run serve para Vue
        // spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
    }
});

app.Run();