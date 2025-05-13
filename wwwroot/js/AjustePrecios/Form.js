@functions {
    private string ParseDetalleToHtml(string detalle)
    {
        var parts = detalle.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var html = "<ul>";
        foreach(var p in parts)
        {
            var sub = p.Split(':');
            var prodID = sub[0];
            var cambios = sub[1].Split(';');

            html += $"<li><strong>ProductoID {prodID}:</strong><ul>";
            foreach(var c in cambios)
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