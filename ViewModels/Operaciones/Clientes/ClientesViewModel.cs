using Javo2.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Javo2.ViewModels.Operaciones.Clientes
{
    public class ClientesViewModel : PersonaBaseViewModel
    {
        public int ClienteID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Range(1000000, 99999999, ErrorMessage = "DNI debe ser un número válido")]
        public int DNI { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio")]
        public string Celular { get; set; } = string.Empty;

        public string TelefonoTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La calle es obligatoria")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de calle es obligatorio")]
        public string NumeroCalle { get; set; } = string.Empty;

        // Datos del domicilio extendidos
        public string NumeroPiso { get; set; } = string.Empty;
        public string Dpto { get; set; } = string.Empty;
        public string TipoDomicilio { get; set; } = string.Empty;
        public int? AntiguedadDomicilio { get; set; }

        [Required(ErrorMessage = "La localidad es obligatoria")]
        public string Localidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código postal es obligatorio")]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del domicilio es obligatoria")]
        public string DescripcionDomicilio { get; set; } = string.Empty;

        // Datos personales extendidos
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        public string EstadoCivil { get; set; } = string.Empty;

        // Datos del cónyuge
        public string NombreConyugue { get; set; } = string.Empty;
        public string ApellidoConyugue { get; set; } = string.Empty;
        public int? DniConyugue { get; set; }
        public string CelularConyugue { get; set; } = string.Empty;
        public string EmailConyugue { get; set; } = string.Empty;
        public string TrabajoConyugue { get; set; } = string.Empty;

        // Datos laborales
        public string Ocupacion { get; set; } = string.Empty;
        public string SituacionLaboral { get; set; } = string.Empty;
        public string LugarTrabajo { get; set; } = string.Empty;
        public string DireccionTrabajo { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public int? AntiguedadLaboral { get; set; }
        public decimal? IngresosMensuales { get; set; }
        public string ReferenciasLaborales { get; set; } = string.Empty;

        // Relación con Provincia y Ciudad
        [Required(ErrorMessage = "La provincia es obligatoria")]
        public int ProvinciaID { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public int CiudadID { get; set; }

        // Datos de crédito
        [Display(Name = "Límite de Crédito Inicial")]
        [Range(0, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor o igual a cero")]
        public decimal LimiteCreditoInicial { get; set; } = 0m;

        [Display(Name = "¿Apto para Crédito?")]
        public bool AptoCredito { get; set; } = false;

        [Display(Name = "¿Requiere Garante?")]
        public bool RequiereGarante { get; set; } = false;

        // Datos de crédito adicionales
        public string ScoreCredito { get; set; } = string.Empty;
        public int? VencimientoCuotas { get; set; }

        // Relación con Garante
        public int? GaranteID { get; set; }
        public string NombreGarante { get; set; } = string.Empty;
        public GaranteViewModel? Garante { get; set; }
        public int ClasificacionCredito { get; set; } // Add this property
        public string TextoClasificacionCredito { get; set; } // Add this property
        public decimal SaldoInicial { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal Saldo { get; set; }
        public string ModificadoPor { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Ciudades { get; set; } = new List<SelectListItem>();

        public bool Verificar { get; set; } = false;
        public string EstadoComentario { get; set; } = string.Empty;

        // Historial
        public List<HistorialCompraViewModel> HistorialCompras { get; set; } = new();
    }
}