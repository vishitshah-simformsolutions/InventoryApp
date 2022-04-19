using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Demo.MedTech.Api.Helpers
{
    /// <summary>
    /// Defines the <see cref="CorrelationIdProvider" />.
    /// </summary>
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        #region Constants

        /// <summary>
        /// Defines the CorrelationIdKey.
        /// </summary>
        private const string CorrelationIdKey = "x-correlation-id";

        #endregion

        #region Fields

        /// <summary>
        /// Defines the _accessor.
        /// </summary>
        private readonly IHttpContextAccessor _accessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdProvider"/> class.
        /// </summary>
        /// <param name="accessor">The accessor<see cref="IHttpContextAccessor"/>.</param>
        public CorrelationIdProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Used to get the correlation id
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetCorrelationId()
        {
            var httpContext = _accessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.ContainsKey(CorrelationIdKey))
            {
                return Convert.ToString(httpContext.Request.Headers[CorrelationIdKey]);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Used to initialize the correlation Id
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string InitializeCorrelationId()
        {
            if (_accessor.HttpContext.Request.Headers.ContainsKey(CorrelationIdKey))
            {
                return _accessor.HttpContext.Request.Headers[CorrelationIdKey];
            }
            else
            {
                StringBuilder correlationId = new StringBuilder();
                string id = correlationId.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                    .Append("SBS")
                    .Append("CRAC")
                    .Append(Guid.NewGuid().ToString("N")[..15])
                    .ToString();

                _accessor.HttpContext.Request.Headers[CorrelationIdKey] = id;
                return id;
            }
        }

        #endregion
    }
}
