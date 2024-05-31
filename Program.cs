using javo2.Services;
using javo2.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(javo2.AutoMapperProfile)); // Asegúrate de usar el espacio de nombres correcto

// Registrar servicios de la aplicación como singleton
builder.Services.AddSingleton<IProductoService, ProductoService>();
builder.Services.AddSingleton<IProveedorService, ProveedorService>();
builder.Services.AddSingleton<IClienteService, ClienteService>();
builder.Services.AddSingleton<IVentaService, VentaService>();
builder.Services.AddSingleton<ICatalogoService, CatalogoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
