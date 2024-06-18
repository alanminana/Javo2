using Javo2.ViewModels.Operaciones.Clientes;

public class ClientesPagedViewModel
{
    public IEnumerable<ClientesViewModel> Clientes { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string SortField { get; set; }
    public bool SortAsc { get; set; }

    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
}
