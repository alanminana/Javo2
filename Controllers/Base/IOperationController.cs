// Controllers/Base/IOperationController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Javo2.Controllers.Base
{
    /// <summary>
    /// Define operaciones comunes para controllers que manejan operaciones de negocio
    /// </summary>
    public interface IOperationController
    {
        /// <summary>
        /// Busca un producto por su código (alfa o barra)
        /// </summary>
        Task<IActionResult> BuscarProductoPorCodigoAsync(string codigoProducto);

        /// <summary>
        /// Busca un cliente por su DNI
        /// </summary>
        Task<IActionResult> BuscarClientePorDNIAsync(int dni);

        /// <summary>
        /// Busca productos por término de búsqueda
        /// </summary>
        Task<IActionResult> BuscarProductosAsync(string term, bool forPurchase = false);
    }
}