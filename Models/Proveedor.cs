// File: Models/Proveedor.cs
namespace Javo2.Models
{
    public class Proveedor
    {
        public int ProveedorID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CondicionesPago { get; set; } = string.Empty;

        // Propiedad agregada para registrar los IDs de los productos asignados a este proveedor.
        public List<int> ProductosAsignados { get; set; } = new List<int>();
    }
}
