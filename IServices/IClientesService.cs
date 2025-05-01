using Javo2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllClientesAsync();
        Task<Cliente?> GetClienteByIDAsync(int id);
        Task<Cliente?> GetClienteByDNIAsync(int dni);
        Task CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);
        Task AgregarCompraAsync(int clienteId, Compra compra);
        Task<IEnumerable<Provincia>> GetProvinciasAsync();
        Task<IEnumerable<Ciudad>> GetCiudadesByProvinciaAsync(int provinciaId);

        // Métodos para crédito y garante
        Task<bool> AsignarGaranteAsync(int clienteId, int garanteId);
        Task<bool> AjustarLimiteCreditoAsync(int clienteId, decimal nuevoLimite, string usuario);
    }
}