
$(function () {
// Cargar ciudades al cambiar provincia
$('#ProvinciaID').change(function () {
var provinciaID = $(this).val();
if (provinciaID) {
$.ajax({
url: '@Url.Action("GetCiudades", "Clientes")',
type: 'GET',
data: { provinciaID: provinciaID },
success: function (ciudades) {
var ciudadSelect = $('#CiudadID');
ciudadSelect.empty();
ciudadSelect.append('<option value="">-- Seleccione --</option>');
$.each(ciudades, function (i, ciudad) {
ciudadSelect.append($('<option></option>').val(ciudad.value).text(ciudad.text));
});
},
error: function () {
alert('Error al cargar ciudades');
}
});
} else {
$('#CiudadID').empty().append('<option value="">-- Seleccione --</option>');
}
});
});
