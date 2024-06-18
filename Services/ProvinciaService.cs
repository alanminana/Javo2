using Javo2.Services;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class ProvinciaService : IProvinciaService
{
    private readonly ILogger<ProvinciaService> _logger;

    public ProvinciaService(ILogger<ProvinciaService> logger)
    {
        _logger = logger;
    }

    private static readonly ConcurrentDictionary<int, ProvinciaViewModel> _provincias = new()
    {
        [1] = new ProvinciaViewModel { ProvinciaID = 1, Nombre = "Buenos Aires" },
        [2] = new ProvinciaViewModel { ProvinciaID = 2, Nombre = "Córdoba" }
    };

    private static readonly ConcurrentBag<CiudadViewModel> _ciudades = new()
    {
        new CiudadViewModel { CiudadID = 1, Nombre = "Lanús", ProvinciaID = 1 },
        new CiudadViewModel { CiudadID = 2, Nombre = "La Plata", ProvinciaID = 1 },
        new CiudadViewModel { CiudadID = 3, Nombre = "Córdoba Capital", ProvinciaID = 2 }
    };

    public IEnumerable<ProvinciaViewModel> GetAllProvincias()
    {
        _logger.LogInformation("GetAllProvincias called");
        return _provincias.Values;
    }

    public IEnumerable<CiudadViewModel> GetCiudadesByProvinciaId(int provinciaId)
    {
        _logger.LogInformation("GetCiudadesByProvinciaId called with ProvinciaID: {ProvinciaID}", provinciaId);
        return _ciudades.Where(c => c.ProvinciaID == provinciaId);
    }
}
