// Models/ConfiguracionSistema.cs (en lugar de Configuracion.cs)
using System.ComponentModel.DataAnnotations;

namespace Javo2.Models
{
    public class ConfiguracionSistema
    {
        public int ConfiguracionID { get; set; }

        [Required]
        public string Modulo { get; set; } = string.Empty;

        [Required]
        public string Clave { get; set; } = string.Empty;

        [Required]
        public string Valor { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string TipoDato { get; set; } = "string"; // string, int, decimal, bool
    }
}