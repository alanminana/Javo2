@functions {
    public string GetNombreAmigable(string clave)
    {
        return clave switch
        {
            "PorcentajeGananciaPLista" => "Porcentaje Ganancia Precio Lista",
            "PorcentajeGananciaPContado" => "Porcentaje Ganancia Precio Contado",
            "LimiteCreditoDefault" => "Límite de Crédito Predeterminado",
            "DiasVencimientoFactura" => "Días para Vencimiento de Facturas",
            _ => clave.Replace("Porcentaje", "% ").Replace("Default", "Predeterminado")
        };
    }
}
