// ViewModels/Operaciones/Productos/AjustePrecioHistoricoViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Productos
{
    public class AjustePrecioHistoricoViewModel
    {
        public int AjusteHistoricoID { get; set; }

        [Display(Name = "Fecha de Ajuste")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaAjuste { get; set; }

        [Display(Name = "Usuario")]
        public string UsuarioAjuste { get; set; }

        [Display(Name = "Porcentaje")]
        [DisplayFormat(DataFormatString = "{0:N2}%")]
        public decimal Porcentaje { get; set; }

        [Display(Name = "Tipo")]
        public bool EsAumento { get; set; }

        [Display(Name = "Tipo de Ajuste")]
        public string TipoAjuste => EsAumento ? "Aumento" : "Descuento";

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Revertido")]
        public bool Revertido { get; set; }

        [Display(Name = "Fecha de Reversión")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaReversion { get; set; }

        [Display(Name = "Usuario Reversión")]
        public string UsuarioReversion { get; set; }

        [Display(Name = "# Productos")]
        public int CantidadProductos => Detalles?.Count ?? 0;

        public List<AjustePrecioDetalleViewModel> Detalles { get; set; } = new List<AjustePrecioDetalleViewModel>();
    }

    public class AjustePrecioDetalleViewModel
    {
        public int DetalleID { get; set; }
        public int AjusteHistoricoID { get; set; }
        public int ProductoID { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "P.Costo Anterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PCostoAnterior { get; set; }

        [Display(Name = "P.Contado Anterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PContadoAnterior { get; set; }

        [Display(Name = "P.Lista Anterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PListaAnterior { get; set; }

        [Display(Name = "P.Costo Posterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PCostoPosterior { get; set; }

        [Display(Name = "P.Contado Posterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PContadoPosterior { get; set; }

        [Display(Name = "P.Lista Posterior")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PListaPosterior { get; set; }

        [Display(Name = "Diferencia P.Costo")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiferenciaPCosto => PCostoPosterior - PCostoAnterior;

        [Display(Name = "Diferencia %")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal PorcentajeCambio => PCostoAnterior != 0 ? (PCostoPosterior / PCostoAnterior) - 1 : 0;
    }
}