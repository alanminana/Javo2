// Services/AjustesTemporalesBackgroundService.cs
using Javo2.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class AjustesTemporalesBackgroundService : BackgroundService
    {
        private readonly ILogger<AjustesTemporalesBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Verificar cada 5 minutos

        public AjustesTemporalesBackgroundService(
            ILogger<AjustesTemporalesBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de monitoreo de ajustes temporales iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                await VerificarAjustesTemporalesAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Servicio de monitoreo de ajustes temporales detenido");
        }

        private async Task VerificarAjustesTemporalesAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var ajustePrecioService = scope.ServiceProvider.GetRequiredService<IAjustePrecioService>();

                    _logger.LogInformation("Iniciando verificación de ajustes temporales");
                    await ajustePrecioService.VerificarYActualizarAjustesTemporalesAsync();
                    _logger.LogInformation("Verificación de ajustes temporales completada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la verificación de ajustes temporales");
            }
        }
    }
}