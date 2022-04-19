using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.MedTech.Api.Domain.Exceptions
{
    /// <summary>
    /// Custom exception thrown in case of a non transient error.
    /// Handled globally in ExceptionMiddleware.
    /// </summary>
    public class NonTransientException : Exception
    {
        public IReadOnlyList<ErrorResult> Errors { get; set; }
        public IReadOnlyDictionary<string, string> HeaderDictionary { get; set; }

        public NonTransientException(string message) : base(message)
        {
        }

        public NonTransientException(IEnumerable<ErrorResult> errors, IReadOnlyDictionary<string, string> headerDictionary = null, string message = nameof(NonTransientException)) :
            this(message)
        {
            Errors = errors?.ToArray();
            HeaderDictionary = headerDictionary;
        }
    }
}