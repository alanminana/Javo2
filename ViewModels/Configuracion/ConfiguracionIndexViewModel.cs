// ViewModels/Operaciones/Configuracion/ConfiguracionIndexViewModel.cs
using Javo2.Models;
using System.Collections.Generic;

namespace Javo2.ViewModels.Configuracion
{
    public class ConfiguracionIndexViewModel
    {
        public List<ConfiguracionSistema> Configuraciones { get; set; } = new List<ConfiguracionSistema>();
        public List<string> Modulos { get; set; } = new List<string>();
        public string ModuloSeleccionado { get; set; }
    }
}