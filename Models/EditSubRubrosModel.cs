// Models/EditSubRubrosModel.cs
using System.Collections.Generic;

namespace Javo2.Models
{
    public class EditSubRubrosModel
    {
        public int RubroID { get; set; }
        public string RubroNombre { get; set; } = string.Empty;
        public List<SubRubroEditModel> SubRubros { get; set; } = new List<SubRubroEditModel>();
    }

    public class SubRubroEditModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
    }
}
