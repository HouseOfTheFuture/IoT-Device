
namespace HouseOfTheFuture.IoTHub.Entities
{
    public class DeviceRegistration
    {
        public DeviceIdentifier DeviceIdentifier { get; set; }
        public string HubDeviceKey { get; set; }
        public bool IsConfigured { get; set; }

        public static DeviceRegistration Current { get; set; }
    }
}