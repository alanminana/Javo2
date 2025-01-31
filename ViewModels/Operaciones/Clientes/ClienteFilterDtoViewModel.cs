// ViewModels/Operaciones/Clientes/ClienteFilterDtoViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class ClienteFilterDtoViewModel
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int? Dni { get; set; }
        public string Email { get; set; }
        public bool? Activo { get; set; }
        public string Localidad { get; set; }
        public DateTime? FechaAltaDesde { get; set; }
        public DateTime? FechaAltaHasta { get; set; }
    }

}
