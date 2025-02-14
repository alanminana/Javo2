// File: Models/CatalogoData.cs
using System.Collections.Generic;

namespace Javo2.Models
{
    public class CatalogoData
    {
        public List<Rubro> Rubros { get; set; } = new List<Rubro>();
        public List<Marca> Marcas { get; set; } = new List<Marca>();
        public int NextRubroID { get; set; } = 1;
        public int NextSubRubroID { get; set; } = 1;
        public int NextMarcaID { get; set; } = 1;
    }
}
