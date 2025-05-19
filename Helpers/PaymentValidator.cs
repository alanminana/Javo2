using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Javo2.Helpers
{
    public static class PaymentValidator
    {
        public static bool ValidatePaymentMethod(VentaFormViewModel model, ModelStateDictionary modelState)
        {
            bool isValid = true;

            // Limpiar errores previos
            modelState.Remove(nameof(model.TipoTarjeta));
            modelState.Remove(nameof(model.Cuotas));
            modelState.Remove(nameof(model.EntidadElectronica));
            modelState.Remove(nameof(model.PlanFinanciamiento));
            modelState.Remove(nameof(model.BancoID));

            // Validar según tipo de pago
            switch (model.FormaPagoID)
            {
                case 2: // Tarjeta de Crédito
                    if (string.IsNullOrEmpty(model.TipoTarjeta))
                    {
                        modelState.AddModelError(nameof(model.TipoTarjeta),
                            "Debe seleccionar un tipo de tarjeta para pagos con tarjeta de crédito.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas.");
                        isValid = false;
                    }
                    if (!model.BancoID.HasValue || model.BancoID.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.BancoID),
                            "Debe seleccionar un banco para tarjeta de crédito.");
                        isValid = false;
                    }
                    break;

                case 3: // Tarjeta de Débito
                    if (!model.BancoID.HasValue || model.BancoID.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.BancoID),
                            "Debe seleccionar un banco para tarjeta de débito.");
                        isValid = false;
                    }
                    break;

                case 4: // Transferencia
                    if (!model.BancoID.HasValue || model.BancoID.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.BancoID),
                            "Debe seleccionar un banco para transferencia.");
                        isValid = false;
                    }
                    break;

                case 5: // Pago Virtual
                    if (string.IsNullOrEmpty(model.EntidadElectronica))
                    {
                        modelState.AddModelError(nameof(model.EntidadElectronica),
                            "Debe seleccionar una entidad electrónica para pagos virtuales.");
                        isValid = false;
                    }
                    break;

                case 6: // Crédito Personal
                    if (string.IsNullOrEmpty(model.PlanFinanciamiento))
                    {
                        modelState.AddModelError(nameof(model.PlanFinanciamiento),
                            "Debe seleccionar un plan de financiamiento para crédito personal.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas para crédito personal.");
                        isValid = false;
                    }
                    break;

                case 7: // Cheque
                    if (string.IsNullOrEmpty(model.NumeroCheque))
                    {
                        modelState.AddModelError(nameof(model.NumeroCheque),
                            "Debe ingresar el número de cheque.");
                        isValid = false;
                    }
                    if (!model.MontoCheque.HasValue || model.MontoCheque.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.MontoCheque),
                            "Debe ingresar el monto del cheque.");
                        isValid = false;
                    }
                    if (!model.BancoID.HasValue || model.BancoID.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.BancoID),
                            "Debe seleccionar el banco emisor del cheque.");
                        isValid = false;
                    }
                    break;
            }

            return isValid;
        }

        // Versión para Cotizaciones
        public static bool ValidatePaymentMethod(CotizacionViewModel model, ModelStateDictionary modelState)
        {
            bool isValid = true;

            // Limpiar errores previos
            modelState.Remove(nameof(model.TipoTarjeta));
            modelState.Remove(nameof(model.Cuotas));
            modelState.Remove(nameof(model.EntidadElectronica));
            modelState.Remove(nameof(model.PlanFinanciamiento));
            modelState.Remove(nameof(model.BancoID));

            // Validar según tipo de pago (mismo código que arriba)
            switch (model.FormaPagoID)
            {
                case 2: // Tarjeta de Crédito
                    if (string.IsNullOrEmpty(model.TipoTarjeta))
                    {
                        modelState.AddModelError(nameof(model.TipoTarjeta),
                            "Debe seleccionar un tipo de tarjeta para pagos con tarjeta de crédito.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas.");
                        isValid = false;
                    }
                    if (!model.BancoID.HasValue || model.BancoID.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.BancoID),
                            "Debe seleccionar un banco para tarjeta de crédito.");
                        isValid = false;
                    }
                    break;

                case 5: // Pago Virtual
                    if (string.IsNullOrEmpty(model.EntidadElectronica))
                    {
                        modelState.AddModelError(nameof(model.EntidadElectronica),
                            "Debe seleccionar una entidad electrónica para pagos virtuales.");
                        isValid = false;
                    }
                    break;

                case 6: // Crédito Personal
                    if (string.IsNullOrEmpty(model.PlanFinanciamiento))
                    {
                        modelState.AddModelError(nameof(model.PlanFinanciamiento),
                            "Debe seleccionar un plan de financiamiento para crédito personal.");
                        isValid = false;
                    }
                    if (!model.Cuotas.HasValue || model.Cuotas.Value <= 0)
                    {
                        modelState.AddModelError(nameof(model.Cuotas),
                            "Debe especificar el número de cuotas para crédito personal.");
                        isValid = false;
                    }
                    break;
            }

            return isValid;
        }
    }
}