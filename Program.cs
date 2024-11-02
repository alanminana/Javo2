// Program.cs
using Javo2.Services;
using Javo2.IServices;
using Javo2.Services.Common;
using Javo2.Middleware;
using Javo2.IServices.Common;
using Microsoft.AspNetCore.Mvc;
using Javo2;

var builder = WebApplication.CreateBuilder(args);

// Configuración del logging
builder.Logging.ClearProviders(); // Limpiar proveedores existentes
builder.Logging.AddConsole();
builder.Logging.AddDebug(); // Añadir el proveedor Debug

// Añadir servicios al contenedor
builder.Services.AddControllersWithViews();

// Registro de servicios con tiempo de vida Scoped
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddSingleton<IClienteService, ClienteService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IProvinciaService, ProvinciaService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();

// Configuración de AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Configuración del pipeline de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Agregar el middleware de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
