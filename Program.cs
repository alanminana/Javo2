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

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Console.WriteLine($"CONTENT ROOT = {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"WEB ROOT     = {builder.Environment.WebRootPath}");

        // Configuración de Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Agregar servicios al contenedor.
        builder.Services.AddControllersWithViews();

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
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Middleware para manejo de excepciones
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        // Middleware de autenticación y autorización
        app.UseAuthenticationConfig();

        // Middleware para verificar autenticación y redirigir a login
        app.Use(async (context, next) =>
        {
            // Permitir acceso a recursos estáticos
            if (context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/img"))
            {
                await next();
                return;
            }

            // Permitir acceso a rutas de autenticación sin estar autenticado
            if (!context.User.Identity.IsAuthenticated &&
                !context.Request.Path.StartsWithSegments("/Auth") &&
                !context.Request.Path.StartsWithSegments("/ResetPassword"))
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await next();
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}