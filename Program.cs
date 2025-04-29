using Javo2.Services;
using Javo2.IServices;
using Javo2.Services.Common;
using Javo2.Middleware;
using Javo2.IServices.Common;
using Javo2.Extensions;
using Javo2.IServices.Authentication;
using Javo2.Services.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Javo2;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews();

// Registro de servicios de la aplicación
// Usar Scoped para servicios que dependen de otros Scoped
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

// Agregar servicios de autenticación personalizados
builder.Services.AddAuthenticationServicesCustom();

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

// Middleware de seguridad
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Configurar autenticación y autorización
app.UseAuthenticationConfigCustom();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();