// Filters/ExceptionHandling/BusinessException.cs
using System;

namespace Javo2.Filters.ExceptionHandling
{
    /// <summary>
    /// Excepción para errores de lógica de negocio
    /// </summary>
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public string UserErrorMessage { get; }

        public BusinessException(string message) : base(message)
        {
            UserErrorMessage = message;
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
            UserErrorMessage = message;
        }

        public BusinessException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
            UserErrorMessage = message;
        }

        public BusinessException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
            UserErrorMessage = message;
        }

        public BusinessException(string message, string errorCode, string userErrorMessage) : base(message)
        {
            ErrorCode = errorCode;
            UserErrorMessage = userErrorMessage ?? message;
        }

        public BusinessException(string message, string errorCode, string userErrorMessage, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
            UserErrorMessage = userErrorMessage ?? message;
        }
    }
}