// Archivo: Services/AuditoriaService.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Javo2.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;
        private static readonly List<AuditoriaRegistro> _registros = new();
        private static int _nextID = 1;

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;
        }

        public Task RegistrarCambioAsync(AuditoriaRegistro registro)
        {
            registro.ID = _nextID++;
            _registros.Add(registro);
            _logger.LogInformation("Se registró auditoría: {@Registro}", registro);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<AuditoriaRegistro>> GetAllRegistrosAsync()
        {
            return Task.FromResult<IEnumerable<AuditoriaRegistro>>(_registros);
        }

        public Task<AuditoriaRegistro?> GetRegistroByIDAsync(int id)
        {
            var reg = _registros.FirstOrDefault(r => r.ID == id);
            return Task.FromResult<AuditoriaRegistro?>(reg);
        }
    }
}
