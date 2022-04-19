using System;
using System.Collections.Generic;

namespace Demo.MedTech.Api.Domain.Exceptions
{
    /// <summary>
    /// Custom exception to be thrown in case of a handled transient exception.
    /// Set the value of RetryAfterSeconds property which will be sent as a 'Retry-After' response header.
    /// Also need to pass the baseException for logging.
    /// Handled globally in ExceptionMiddleware.
    /// </summary>
    public class TransientException : Exception
    {
        public uint RetryAfterSeconds { get; set; }
        public IReadOnlyDictionary<string, string> HeaderDictionary { get; set; }

        public TransientException(uint retryAfterSeconds, Exception baseException, string message = nameof(TransientException), IReadOnlyDictionary<string, string> headerDictionary = null)
            : base(message, baseException)
        {
            RetryAfterSeconds = retryAfterSeconds;
            HeaderDictionary = headerDictionary;
        }
    }
}