using System.Collections.Generic;

namespace TryOmnisharpExtension.FindImplementations
{
    public class FindImplementationsResponse
    {
        public FindImplementationsResponse()
        {
            Implementations = new List<ResponseLocation>();
        }
        public IList<ResponseLocation> Implementations { get; }
    }
}