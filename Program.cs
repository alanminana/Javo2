using Javo2.Services;
using Javo2.IServices;
using Javo2.Services.Common;
using Javo2.Middleware;
using Javo2.IServices.Common;
using Javo2.Extensions;
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
builder.Services.AddScoped<IDevolucionGarantiaService, DevolucionGarantiaService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Servicio de búsqueda de cliente
builder.Services.AddScoped<IClienteSearchService>(sp =>
    sp.GetRequiredService<IClienteService>() as IClienteSearchService
    ?? throw new InvalidOperationException("ClienteService must implement IClienteSearchService"));

// Otros servicios
builder.Services.AddScoped<IDropdownService, DropdownService>();

// Configuración de autenticación
builder.Services.AddAuthenticationServices();

// Configuración de AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Validación de AutoMapper
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

// Middlewares personalizados
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Autenticación y autorización
app.UseAuthenticationConfig();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();