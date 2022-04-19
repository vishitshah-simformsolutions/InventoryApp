using System.Collections.Generic;

namespace Product.Api.Domain.Shared
{
    public class Headers
    {
        public IList<string> Request { get; set; } = new List<string>();
    }
}