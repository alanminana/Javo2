using Javo2.Services;
using Javo2.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(Javo2.AutoMapperProfile));

builder.Services.AddSingleton<IProductoService, ProductoService>();
builder.Services.AddSingleton<IProveedorService, ProveedorService>();
builder.Services.AddSingleton<IClienteService, ClienteService>();
builder.Services.AddSingleton<IVentaService, VentaService>();
builder.Services.AddSingleton<ICatalogoService, CatalogoService>();
builder.Services.AddSingleton<IProvinciaService, ProvinciaService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
