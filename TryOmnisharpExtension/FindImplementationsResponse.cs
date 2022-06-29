using System.Collections.Generic;

namespace TryOmnisharpExtension
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