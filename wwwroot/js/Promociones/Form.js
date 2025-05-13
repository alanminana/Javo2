@model Javo2.ViewModels.Operaciones.Promociones.PromocionViewModel

@{
    var isEdit = Model.PromocionID > 0;
    ViewBag.Title = isEdit ? "Editar Promoción" : "Crear Promoción";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h2 class="mb-4">@ViewBag.Title</h2>

<div class="card p-4">
    <form asp-action="@(isEdit ? "Edit" : "Create")" method="post">
        @Html.AntiForgeryToken()

        <input type="hidden" asp-for="PromocionID" />

        <div class="mb-3">
            <label asp-for="Nombre" class="form-label"></label>
            <input asp-for="Nombre" class="form-control" />
            <span asp-validation-for="Nombre" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="Descripcion" class="form-label"></label>
            <input asp-for="Descripcion" class="form-control" />
            <span asp-validation-for="Descripcion" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="Porcentaje" class="form-label"></label>
            <input asp-for="Porcentaje" class="form-control" />
            <span asp-validation-for="Porcentaje" class="text-danger"></span>
        </div>
        <div class="mb-3 form-check">
            <input asp-for="EsAumento" class="form-check-input" />
            <label asp-for="EsAumento" class="form-check-label">Es Aumento (si no, es descuento)</label>
        </div>

        <div class="mb-3">
            <label asp-for="RubroID">Rubro ID (opcional)</label>
            <input asp-for="RubroID" class="form-control" />
        </div>
        <div class="mb-3">
            <label asp-for="MarcaID">Marca ID (opcional)</label>
            <input asp-for="MarcaID" class="form-control" />
        </div>
        <div class="mb-3">
            <label asp-for="SubRubroID">SubRubro ID (opcional)</label>
            <input asp-for="SubRubroID" class="form-control" />
        </div>
        <div class="mb-3">
            <label asp-for="FechaInicio">Fecha Inicio (opcional)</label>
            <input asp-for="FechaInicio" class="form-control" type="datetime-local" />
        </div>
        <div class="mb-3">
            <label asp-for="FechaFin">Fecha Fin (opcional)</label>
            <input asp-for="FechaFin" class="form-control" type="datetime-local" />
        </div>
        <div class="mb-3 form-check">
            <input asp-for="Activa" class="form-check-input" />
            <label asp-for="Activa" class="form-check-label">Activa</label>
        </div>

        <button type="submit" class="btn btn-primary me-2">
            <i class="bi bi-save me-2"></i> @(isEdit ? "Actualizar" : "Crear")
        </button>
        <a asp-action="Index" class="btn btn-secondary">
            <i class="bi bi-arrow-left me-2"></i> Volver al Listado
        </a>
    </form>
</div>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
