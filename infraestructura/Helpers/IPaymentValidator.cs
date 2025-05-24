namespace Javo2.infraestructura.Helpers
{
    public interface IPaymentModel
    {
        int FormaPagoID { get; }
        string TipoTarjeta { get; }
        int? Cuotas { get; }
        int? BancoID { get; }
        string EntidadElectronica { get; }
        string PlanFinanciamiento { get; }
        string NumeroCheque { get; }
        decimal? MontoCheque { get; }
    }
}
