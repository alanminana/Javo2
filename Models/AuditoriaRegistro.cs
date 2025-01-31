// Models/AuditoriaRegistro.cs
using System;

namespace Javo2.Models
{
    public class AuditoriaRegistro
    {
        public int ID { get; set; }
        public DateTime FechaHora { get; set; }
        public string Usuario { get; set; } = "";
        public string Entidad { get; set; } = "";
        public string Accion { get; set; } = "";
        public string LlavePrimaria { get; set; } = "";
        public string Detalle { get; set; } = "";

        public bool EsRevertido { get; set; } = false;
        public string RollbackUser { get; set; } = "";
        public DateTime? RollbackFecha { get; set; }
    }
}
