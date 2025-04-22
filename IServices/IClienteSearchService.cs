using Javo2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IClienteSearchService
    {
        Task<(IEnumerable<Cliente> Clientes, int TotalCount)> SearchClientesAsync(
            string? searchTerm = null,
            int? page = null,
            int? pageSize = null,
            string? orderBy = null,
            bool? ascending = true
        );
    }
}