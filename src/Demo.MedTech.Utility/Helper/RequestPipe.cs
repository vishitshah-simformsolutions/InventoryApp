using System.Collections.Generic;

namespace Demo.MedTech.Utility.Helper
{
    /// <summary>
    /// Request pipe is used for persisting values throughout the request.
    /// These values can be accessed in any middleware, controller, handler, validator, etc. using dependency injection.
    /// More properties can be added here for persistence as required.
    /// </summary>
    public class RequestPipe : IRequestPipe
    {
        public RequestPipe()
        {
            AdditionalHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Any additional headers to be passed in the response should be added to this dictionary.
        /// Used for passing custom response headers in ResponseMiddleware.
        /// </summary>
        public Dictionary<string, string> AdditionalHeaders { get; set; }

        /// <summary>
        /// Correlation id passed in the request headers.
        /// Used for passing values in LogHelper for logging purposes.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Reads value of response model from header for standard responses in place bid api
        /// </summary>
        public string ResponseModel { get; set; }
    }
}