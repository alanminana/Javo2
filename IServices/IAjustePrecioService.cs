// File: IServices/IAjustePrecioService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Javo2.IServices
{
    public interface IAjustePrecioService
    {
        /// <summary>
        /// Ajusta los precios de los productos indicados.
        /// </summary>
        /// <param name="productoIDs">Lista de IDs de productos a ajustar.</param>
        /// <param name="porcentaje">Porcentaje de ajuste.</param>
        /// <param name="esAumento">Si es true, aumenta el precio; si es false, lo disminuye.</param>
        Task AjustarPreciosAsync(IEnumerable<int> productoIDs, decimal porcentaje, bool esAumento);
    }
}
