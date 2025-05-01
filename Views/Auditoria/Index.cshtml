@model IEnumerable<Javo2.Models.AuditoriaRegistro>

@{
    ViewBag.Title = "Auditoría - Historial de Cambios";
    Layout = "~/Views/Shared/_Layout.cshtml";

    // Determina si el usuario tiene permiso de rollback
    bool canRollback = User.HasPermission("auditoria.rollback");
}

<h2 class="mb-4">@ViewBag.Title</h2>

@if (TempData["Error"] != null)
{
    <div class="alert alert-danger">@TempData["Error"]</div>
}

@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}

<table class="table table-dark table-striped">
    <thead>
        <tr>
            <th>Fecha/Hora</th>
            <th>Usuario</th>
            <th>Entidad</th>
            <th>Acción</th>
            <th>Llave</th>
            <th>Detalle</th>
            @* Sólo mostramos la columna de Rollback si el usuario puede revertir *@
            @if (canRollback)
            {
                <th>Rollback</th>
            }
        </tr>
    </thead>
    <tbody>
        @foreach (var reg in Model)
        {
            var changesHtml = reg.Detalle;
            if (reg.Entidad == "Producto" && reg.Accion == "UpdatePrices")
            {
                changesHtml = ParseDetalleToHtml(reg.Detalle);
            }

            <tr>
                <td>@reg.FechaHora</td>
                <td>@reg.Usuario</td>
                <td>@reg.Entidad</td>
                <td>@reg.Accion</td>
                <td>@reg.LlavePrimaria</td>
                <td>
                    <pre class="text-white">@Html.Raw(changesHtml)</pre>
                </td>
                @* Sólo renderizamos la celda de Rollback si el usuario tiene permiso *@
                @if (canRollback)
                {
                    <td>
                        @if (reg.EsRevertido)
                        {
                            <span class="badge bg-info text-dark">
                                Revertido por @reg.RollbackUser (@reg.RollbackFecha.Value.ToShortDateString())
                            </span>
                        }
                        else if (reg.Entidad == "Producto" && reg.Accion == "UpdatePrices")
                        {
                            <form asp-action="Rollback" method="post" class="d-inline">
                                <input type="hidden" name="id" value="@reg.ID" />
                                <button type="submit"
                                        class="btn btn-warning btn-sm"
                                        onclick="return confirm('¿Desea revertir este cambio?');">
                                    Revertir
                                </button>
                            </form>
                        }
                    </td>
                }
            </tr>
        }
    </tbody>
</table>

@functions {
    private string ParseDetalleToHtml(string detalle)
    {
        var parts = detalle.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var html = "<ul>";
        foreach (var p in parts)
        {
            var sub = p.Split(':');
            var prodID = sub[0];
            var cambios = sub[1].Split(';');

            html += $"<li><strong>ProductoID {prodID}:</strong><ul>";
            foreach (var c in cambios)
            {
                var eq = c.Split('=');
                var campo = eq[0];
                var vals = eq[1].Split("->");
                html += $"<li>{campo}: {vals[0]} → {vals[1]}</li>";
            }
            html += "</ul></li>";
        }
        html += "</ul>";
        return html;
    }
}
