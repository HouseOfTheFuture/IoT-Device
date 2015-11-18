using System.Linq;
using Windows.Networking.Connectivity;

namespace HouseOfTheFuture.IoTHub.Helpers
{
    class NetworkInterface
    {
        public static string[] IpAddresses
        {
            get
            {
                return NetworkInformation.GetHostNames()
                    .Where(x => x.IPInformation != null
                                && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                                    || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6)) // ethernet
                    .Select(x => x.DisplayName).ToArray();
            }
        }
    }
}