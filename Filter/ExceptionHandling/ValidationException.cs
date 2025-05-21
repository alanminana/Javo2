// Filters/ExceptionHandling/ValidationException.cs
using System;
using System.Collections.Generic;

namespace Javo2.Filters.ExceptionHandling
{
    /// <summary>
    /// Excepción para errores de validación
    /// </summary>
    public class ValidationException : Exception
    {
        public Dictionary<string, List<string>> ValidationErrors { get; }

        public ValidationException(string message) : base(message)
        {
            ValidationErrors = new Dictionary<string, List<string>>();
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = new Dictionary<string, List<string>>();
        }

        public ValidationException(string message, Dictionary<string, List<string>> validationErrors) : base(message)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, List<string>>();
        }

        public ValidationException(string message, Dictionary<string, List<string>> validationErrors, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, List<string>>();
        }

        public void AddError(string key, string errorMessage)
        {
            if (!ValidationErrors.ContainsKey(key))
            {
                ValidationErrors[key] = new List<string>();
            }

            ValidationErrors[key].Add(errorMessage);
        }
    }
}