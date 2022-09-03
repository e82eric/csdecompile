using System.Collections.Generic;

namespace CsDecompileLib
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