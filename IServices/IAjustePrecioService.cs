// IServices/IAjustePrecioService.cs
using Javo2.Models;
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
        /// <param name="descripcion">Descripción opcional del ajuste.</param>
        /// <returns>ID del registro histórico creado</returns>
        Task<int> AjustarPreciosAsync(IEnumerable<int> productoIDs, decimal porcentaje, bool esAumento, string descripcion = "");

        /// <summary>
        /// Obtiene el historial de ajustes de precios.
        /// </summary>
        Task<IEnumerable<AjustePrecioHistorico>> ObtenerHistorialAjustesAsync();

        /// <summary>
        /// Obtiene un ajuste histórico por su ID.
        /// </summary>
        Task<AjustePrecioHistorico> ObtenerAjusteHistoricoAsync(int ajusteHistoricoID);

        /// <summary>
        /// Revierte un ajuste de precios.
        /// </summary>
        Task RevertirAjusteAsync(int ajusteHistoricoID, string usuario);
    }
}