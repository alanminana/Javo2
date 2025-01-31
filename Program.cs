// Archivo: Program.cs

using Javo2.Services;
using Javo2.IServices;
using Javo2.Services.Common;
using Javo2.Middleware;
using Javo2.IServices.Common;
using AutoMapper;
using Javo2;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews();

// Registro de servicios de la aplicación
builder.Services.AddSingleton<IProductoService, ProductoService>();
builder.Services.AddSingleton<IProveedorService, ProveedorService>();
builder.Services.AddSingleton<ICatalogoService, CatalogoService>();
builder.Services.AddSingleton<IProvinciaService, ProvinciaService>();
builder.Services.AddSingleton<IStockService, StockService>();
builder.Services.AddSingleton<IClienteService, ClienteService>();
builder.Services.AddSingleton<IPromocionesService, PromocionesService>();
builder.Services.AddSingleton<IAuditoriaService, AuditoriaService>();
builder.Services.AddSingleton<IVentaService, VentaService>();
builder.Services.AddSingleton<IProductoService, ProductoService>();
builder.Services.AddSingleton<ICotizacionService, CotizacionService>();
builder.Services.AddSingleton<IStockService, StockService>();
builder.Services.AddSingleton<IPromocionService, PromocionService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IReporteService, ReporteService>();
// Servicios con ciclo de vida Scoped
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();

// Registro de IAuditoriaService
builder.Services.AddSingleton<IAuditoriaService, AuditoriaService>();

// Configuración de AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Build de la aplicación
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
    // Opcional: detener la aplicación si hay errores de mapeo
    // throw;
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

// Middleware personalizado para manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
