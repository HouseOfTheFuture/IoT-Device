using System.Collections.Generic;
using Windows.Networking.Connectivity;

namespace HouseOfTheFuture.IoTHub.Helpers
{
    internal class NetworkAdapterComparer 
        : IEqualityComparer<NetworkAdapter>
    {
        public bool Equals(NetworkAdapter x, NetworkAdapter y)
        {
            return Equals(x.NetworkAdapterId, y.NetworkAdapterId);
        }

        public int GetHashCode(NetworkAdapter obj)
        {
            return obj.NetworkAdapterId.GetHashCode();
        }
    }
}