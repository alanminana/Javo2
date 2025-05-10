// IServices/IAjustePrecioService.cs (Ampliado)
using Javo2.Models;
using System;
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

        /// <summary>
        /// Crea un ajuste de precios temporal que se aplicará durante un período específico.
        /// </summary>
        /// <param name="productoIDs">Lista de IDs de productos a ajustar.</param>
        /// <param name="porcentaje">Porcentaje de ajuste.</param>
        /// <param name="esAumento">Si es true, aumenta el precio; si es false, lo disminuye.</param>
        /// <param name="fechaInicio">Fecha de inicio del ajuste temporal.</param>
        /// <param name="fechaFin">Fecha de fin del ajuste temporal.</param>
        /// <param name="tipoAjuste">Tipo de ajuste (Promoción, Hot Sale, etc.).</param>
        /// <param name="descripcion">Descripción opcional del ajuste.</param>
        /// <returns>ID del registro histórico creado</returns>
        Task<int> CrearAjusteTemporalAsync(
            IEnumerable<int> productoIDs,
            decimal porcentaje,
            bool esAumento,
            DateTime fechaInicio,
            DateTime fechaFin,
            string tipoAjuste,
            string descripcion = "");

        /// <summary>
        /// Obtiene los ajustes temporales activos y programados.
        /// </summary>
        Task<IEnumerable<AjustePrecioHistorico>> ObtenerAjustesTemporalesActivosAsync();

        /// <summary>
        /// Obtiene los ajustes temporales por estado.
        /// </summary>
        Task<IEnumerable<AjustePrecioHistorico>> ObtenerAjustesTemporalesPorEstadoAsync(EstadoAjusteTemporal estado);

        /// <summary>
        /// Activa un ajuste temporal programado.
        /// </summary>
        Task ActivarAjusteTemporalAsync(int ajusteHistoricoID);

        /// <summary>
        /// Finaliza y revierte un ajuste temporal activo.
        /// </summary>
        Task FinalizarAjusteTemporalAsync(int ajusteHistoricoID, string usuario);

        /// <summary>
        /// Verifica y actualiza el estado de todos los ajustes temporales.
        /// Este método debe ser llamado por un job programado regularmente.
        /// </summary>
        Task VerificarYActualizarAjustesTemporalesAsync();
    }
}