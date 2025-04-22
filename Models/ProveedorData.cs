// Archivo: Models/ProveedorData.cs
// Se mantiene igual ya que es necesario para la persistencia

using System.Collections.Generic;

namespace Javo2.Models
{
    public class ProveedorData
    {
        public List<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
    }
}