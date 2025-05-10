// Models/AjustePrecioHistorico.cs
using System;
using System.Collections.Generic;

namespace Javo2.Models
{
    public class AjustePrecioHistorico
    {
        public int AjusteHistoricoID { get; set; }
        public DateTime FechaAjuste { get; set; } = DateTime.Now;
        public string UsuarioAjuste { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EsAumento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public List<AjustePrecioDetalle> Detalles { get; set; } = new List<AjustePrecioDetalle>();
        public bool Revertido { get; set; } = false;
        public DateTime? FechaReversion { get; set; }
        public string UsuarioReversion { get; set; } = string.Empty;
    }

    public class AjustePrecioDetalle
    {
        public int DetalleID { get; set; }
        public int AjusteHistoricoID { get; set; }
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal PCostoAnterior { get; set; }
        public decimal PContadoAnterior { get; set; }
        public decimal PListaAnterior { get; set; }
        public decimal PCostoPosterior { get; set; }
        public decimal PContadoPosterior { get; set; }
        public decimal PListaPosterior { get; set; }
    }
}