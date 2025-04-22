using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class ClientesService : IClienteService
    {
        private readonly string _jsonFilePath = Path.Combine("Data", "clientes.json");
        private readonly IJsonFileHelper _fileHelper;
        private readonly object _lock = new object();

        public ClientesService(IJsonFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        public async Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            var json = await _fileHelper.ReadAsync(_jsonFilePath);
            return JsonSerializer.Deserialize<List<Cliente>>(json) ?? new List<Cliente>();
        }

        public async Task<Cliente?> GetClienteByIDAsync(int id)
        {
            return (await GetAllClientesAsync()).FirstOrDefault(c => c.ClienteID == id);
        }

        public async Task CreateClienteAsync(Cliente cliente)
        {
            var lista = (await GetAllClientesAsync()).ToList();
            cliente.ClienteID = lista.Any() ? lista.Max(c => c.ClienteID) + 1 : 1;
            lista.Add(cliente);
            var json = JsonSerializer.Serialize(lista);
            await _fileHelper.WriteAsync(_jsonFilePath, json);
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            var lista = (await GetAllClientesAsync()).ToList();
            var index = lista.FindIndex(c => c.ClienteID == cliente.ClienteID);
            if (index >= 0)
            {
                lista[index] = cliente;
                var json = JsonSerializer.Serialize(lista);
                await _fileHelper.WriteAsync(_jsonFilePath, json);
            }
        }

        public async Task DeleteClienteAsync(int id)
        {
            var lista = (await GetAllClientesAsync()).ToList();
            lista.RemoveAll(c => c.ClienteID == id);
            var json = JsonSerializer.Serialize(lista);
            await _fileHelper.WriteAsync(_jsonFilePath, json);
        }

        public async Task<IEnumerable<Provincia>> GetProvinciasAsync()
        {
            var json = await _fileHelper.ReadAsync(Path.Combine("Data", "provincias.json"));
            return JsonSerializer.Deserialize<List<Provincia>>(json) ?? new List<Provincia>();
        }

        public async Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaID)
        {
            var json = await _fileHelper.ReadAsync(Path.Combine("Data", "ciudades.json"));
            var ciudades = JsonSerializer.Deserialize<List<Ciudad>>(json) ?? new List<Ciudad>();
            return ciudades.Where(c => c.ProvinciaID == provinciaID);
        }

        public async Task<Cliente?> GetClienteByDNIAsync(int dni) =>
            (await GetAllClientesAsync()).FirstOrDefault(c => c.DNI == dni);

        public async Task AgregarCompraAsync(int clienteID, Compra compra)
        {
            var cliente = await GetClienteByIDAsync(clienteID);
            if (cliente != null)
            {
                cliente.Compras ??= new List<Compra>();
                cliente.Compras.Add(compra);
                await UpdateClienteAsync(cliente);
            }
        }
    }
}