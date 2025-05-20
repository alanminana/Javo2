// Services/CreditoService.cs
using Javo2.Helpers;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Services
{
    public class CreditoService : ICreditoService
    {
        private readonly ILogger<CreditoService> _logger;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IClienteService _clienteService;
        private readonly string _configuracionPath = "Data/configuracion_credito.json";
        private readonly string _criteriosPath = "Data/criterios_credito.json";

        private ConfiguracionCredito _configuracionVigente;
        private List<CriteriosCalificacionCredito> _criterios;

        public CreditoService(
            ILogger<CreditoService> logger,
            IAuditoriaService auditoriaService,
            IClienteService clienteService)
        {
            _logger = logger;
            _auditoriaService = auditoriaService;
            _clienteService = clienteService;

            // Cargar configuración
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Cargar configuración
                if (File.Exists(_configuracionPath))
                {
                    _configuracionVigente = await JsonFileHelper.LoadFromJsonFileAsync<ConfiguracionCredito>(_configuracionPath);
                }
                else
                {
                    // Crear configuración por defecto
                    _configuracionVigente = new ConfiguracionCredito
                    {
                        Nombre = "Configuración Inicial",
                        PorcentajeRecargoBase = 10,
                        RecargoScoreA = 5,
                        RecargoScoreB = 10,
                        RecargoScoreC = 15,
                        RecargoScoreD = 20,
                        PorcentajeMoraDiaria = 0.1M,
                        PorcentajeMoraMensual = 3M
                    };
                    await JsonFileHelper.SaveToJsonFileAsync(_configuracionPath, _configuracionVigente);
                }

                // Cargar criterios
                if (File.Exists(_criteriosPath))
                {
                    _criterios = await JsonFileHelper.LoadFromJsonFileAsync<List<CriteriosCalificacionCredito>>(_criteriosPath);
                }
                else
                {
                    // Crear criterios por defecto
                    _criterios = new List<CriteriosCalificacionCredito>
                    {
                        new CriteriosCalificacionCredito { ScoreCredito = "A", Descripcion = "Excelente", AptoCredito = true, RequiereGarante = false, LimiteCreditoMaximo = 100000, PlazoMaximo = 24 },
                        new CriteriosCalificacionCredito { ScoreCredito = "B", Descripcion = "Bueno", AptoCredito = true, RequiereGarante = false, LimiteCreditoMaximo = 50000, PlazoMaximo = 18 },
                        new CriteriosCalificacionCredito { ScoreCredito = "C", Descripcion = "Regular", AptoCredito = true, RequiereGarante = true, LimiteCreditoMaximo = 25000, PlazoMaximo = 12 },
                        new CriteriosCalificacionCredito { ScoreCredito = "D", Descripcion = "Malo", AptoCredito = true, RequiereGarante = true, LimiteCreditoMaximo = 10000, PlazoMaximo = 6 }
                    };
                    await JsonFileHelper.SaveToJsonFileAsync(_criteriosPath, _criterios);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar el servicio de crédito");
                throw;
            }
        }

        public async Task<ConfiguracionCredito> GetConfiguracionVigenteAsync()
        {
            return _configuracionVigente;
        }

        public async Task<ConfiguracionCredito> SaveConfiguracionAsync(ConfiguracionCredito config)
        {
            try
            {
                config.FechaModificacion = DateTime.Now;
                _configuracionVigente = config;
                await JsonFileHelper.SaveToJsonFileAsync(_configuracionPath, config);

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = config.ModificadoPor,
                    Entidad = "ConfiguracionCredito",
                    Accion = "Update",
                    LlavePrimaria = config.ConfiguracionCreditoID.ToString(),
                    Detalle = $"Configuración de crédito actualizada: {config.Nombre}"
                });

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de crédito");
                throw;
            }
        }

        public async Task<CriteriosCalificacionCredito> GetCriterioByScoreAsync(string score)
        {
            return _criterios.FirstOrDefault(c => c.ScoreCredito == score && c.Activo);
        }

        public async Task<IEnumerable<CriteriosCalificacionCredito>> GetAllCriteriosAsync()
        {
            return _criterios.Where(c => c.Activo);
        }

        public async Task<CriteriosCalificacionCredito> SaveCriterioAsync(CriteriosCalificacionCredito criterio)
        {
            try
            {
                criterio.FechaModificacion = DateTime.Now;

                var existente = _criterios.FirstOrDefault(c => c.CriterioID == criterio.CriterioID);
                if (existente != null)
                {
                    // Actualizar
                    _criterios.Remove(existente);
                }

                _criterios.Add(criterio);
                await JsonFileHelper.SaveToJsonFileAsync(_criteriosPath, _criterios);

                return criterio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar criterio de crédito");
                throw;
            }
        }

        public async Task<decimal> CalcularRecargoAsync(string scoreCredito, decimal montoTotal, int numeroCuotas)
        {
            var config = await GetConfiguracionVigenteAsync();
            var criterio = await GetCriterioByScoreAsync(scoreCredito);

            if (criterio == null)
            {
                _logger.LogWarning("No se encontró criterio para score: {Score}", scoreCredito);
                return config.RecargoScoreD;
            }

            // Obtener recargo base según score
            decimal recargoBase = scoreCredito switch
            {
                "A" => config.RecargoScoreA,
                "B" => config.RecargoScoreB,
                "C" => config.RecargoScoreC,
                "D" => config.RecargoScoreD,
                _ => config.RecargoScoreD
            };

            // Ajustar recargo según número de cuotas
            // A más cuotas, mayor recargo
            decimal factorCuotas = 1 + ((decimal)numeroCuotas / 24); // 24 cuotas como máximo estándar

            return recargoBase * factorCuotas;
        }

        public async Task<decimal> CalcularInteresDeAtrasoAsync(int diasAtraso, decimal montoCuota)
        {
            var config = await GetConfiguracionVigenteAsync();

            if (diasAtraso <= config.DiasGracia)
                return 0;

            // Calcular interés diario
            decimal interesDiario = config.PorcentajeMoraDiaria / 100;
            return montoCuota * interesDiario * diasAtraso;
        }

        public async Task<List<Cuota>> GenerarCuotasAsync(Venta venta, int numeroCuotas, DateTime primerVencimiento)
        {
            var cuotas = new List<Cuota>();

            // Validar que la venta esté en un estado válido
            if (venta.Estado != EstadoVenta.Autorizada)
            {
                _logger.LogWarning("No se pueden generar cuotas para una venta no autorizada. VentaID: {VentaID}", venta.VentaID);
                return cuotas;
            }

            // Obtener cliente y garante
            var cliente = await _clienteService.GetClienteByIDAsync(venta.DniCliente);
            if (cliente == null)
            {
                _logger.LogWarning("No se encontró el cliente para la venta. VentaID: {VentaID}", venta.VentaID);
                return cuotas;
            }

            // Calcular monto de cada cuota (capital + interés)
            decimal montoTotalConRecargo = venta.PrecioTotal;
            decimal montoCuota = montoTotalConRecargo / numeroCuotas;
            decimal montoCapitalCuota = venta.TotalSinRecargo / numeroCuotas;
            decimal montoInteresCuota = montoCuota - montoCapitalCuota;

            // Generar cuotas
            DateTime fechaVencimiento = primerVencimiento;
            for (int i = 1; i <= numeroCuotas; i++)
            {
                var cuota = new Cuota
                {
                    VentaID = venta.VentaID,
                    ClienteID = cliente.ClienteID,
                    GaranteID = cliente.GaranteID,
                    NumeroCuota = i,
                    FechaVencimiento = fechaVencimiento,
                    ImporteCuota = montoCuota,
                    MontoCapital = montoCapitalCuota,
                    MontoInteres = montoInteresCuota,
                    EstadoCuota = EstadoCuota.Pendiente,
                    FechaCreacion = DateTime.Now,
                    ModificadoPor = venta.Usuario
                };

                cuotas.Add(cuota);

                // Calcular próximo vencimiento (mensual)
                fechaVencimiento = fechaVencimiento.AddMonths(1);
            }

            return cuotas;
        }

        public async Task<bool> RegistrarPagoCuotaAsync(int cuotaId, decimal monto, string formaPago, string referencia, string usuario)
        {
            // Implementar lógica para registrar pago
            // Este método debería:
            // 1. Buscar la cuota
            // 2. Verificar que no esté pagada
            // 3. Registrar el pago
            // 4. Actualizar el estado
            // 5. Actualizar saldo pendiente de la venta

            // Implementación depende de cómo se almacenan las cuotas

            return true;
        }

        public async Task<bool> AsignarCuotaAGaranteAsync(int cuotaId, string usuario)
        {
            // Implementar lógica para transferir responsabilidad al garante

            return true;
        }

        public async Task<List<Cuota>> GetCuotasPendientesByClienteAsync(int clienteId)
        {
            // Implementar consulta de cuotas pendientes

            return new List<Cuota>();
        }

        public async Task<List<Cuota>> GetCuotasVencidasAsync(int diasAtraso = 0)
        {
            // Implementar consulta de cuotas vencidas

            return new List<Cuota>();
        }

        public async Task<bool> ClienteCalificaPorCreditoAsync(int clienteId, decimal montoTotal, int numeroCuotas)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(clienteId);
            if (cliente == null || !cliente.AptoCredito)
                return false;

            var criterio = await GetCriterioByScoreAsync(cliente.ScoreCredito);
            if (criterio == null || !criterio.AptoCredito)
                return false;

            // Verificar límite de crédito
            if (montoTotal > criterio.LimiteCreditoMaximo)
                return false;

            // Verificar plazo máximo
            if (numeroCuotas > criterio.PlazoMaximo)
                return false;

            // Verificar si requiere garante
            if (criterio.RequiereGarante && (!cliente.GaranteID.HasValue || cliente.GaranteID.Value <= 0))
                return false;

            return true;
        }
    }
}