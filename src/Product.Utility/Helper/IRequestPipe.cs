using System.Collections.Generic;

namespace Product.Utility.Helper
{
    public interface IRequestPipe
    {
        Dictionary<string, string> AdditionalHeaders { get; set; }
        string CorrelationId { get; set; }
        string ResponseModel { get; set; }
    }
}