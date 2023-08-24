using System.Collections.Generic;

namespace CsDecompileLib
{
    public class LocationsResponse
    {
        public LocationsResponse()
        {
            Locations = new List<ResponseLocation>();
        }
        public IList<ResponseLocation> Locations { get; }
    }
}