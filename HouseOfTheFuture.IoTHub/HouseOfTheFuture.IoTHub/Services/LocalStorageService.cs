using Windows.Storage;
using HouseOfTheFuture.IoTHub.Entities;

namespace HouseOfTheFuture.IoTHub.Services
{
    public interface ILocalStorageService
    {
        DeviceIdentifier? CheckForExistingDeviceIdentifier();
        void PersistDeviceIdentifier(DeviceIdentifier deviceIdentifier);
    }

    public class LocalStorageService
        : ILocalStorageService
    {
        private const string FileName = "deviceconfig.json";

        public DeviceIdentifier? CheckForExistingDeviceIdentifier()
        {
            var id = (string) ApplicationData.Current.LocalSettings.Values["deviceconfig"];

            if (!string.IsNullOrEmpty(id))
            {
                return new DeviceIdentifier(id);
            }

            return null;
        }

        public void PersistDeviceIdentifier(DeviceIdentifier deviceIdentifier)
        {
            ApplicationData.Current.LocalSettings.Values["deviceconfig"] = deviceIdentifier.ToString();
        }
    }
}    