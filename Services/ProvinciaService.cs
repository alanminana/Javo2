using Javo2.IServices;
using Javo2.Models;

public class ProvinciaService : IProvinciaService
{
    private readonly List<Provincia> _provincias;
    private readonly List<Ciudad> _ciudades;

    public ProvinciaService()
    {
        // Datos de ejemplo
        _provincias = new List<Provincia>
        {
            new Provincia { ProvinciaID = 1, Nombre = "Provincia 1" },
            new Provincia { ProvinciaID = 2, Nombre = "Provincia 2" }
        };

        _ciudades = new List<Ciudad>
        {
            new Ciudad { CiudadID = 1, Nombre = "Ciudad A", ProvinciaID = 1 },
            new Ciudad { CiudadID = 2, Nombre = "Ciudad B", ProvinciaID = 1 },
            new Ciudad { CiudadID = 3, Nombre = "Ciudad C", ProvinciaID = 2 },
            new Ciudad { CiudadID = 4, Nombre = "Ciudad D", ProvinciaID = 2 }
        };
    }

    public Task<IEnumerable<Provincia>> GetAllProvinciasAsync()
    {
        return Task.FromResult(_provincias.AsEnumerable());
    }

    public Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaIDAsync(int provinciaID)
    {
        var ciudades = _ciudades.Where(c => c.ProvinciaID == provinciaID);
        return Task.FromResult(ciudades.AsEnumerable());
    }
}
