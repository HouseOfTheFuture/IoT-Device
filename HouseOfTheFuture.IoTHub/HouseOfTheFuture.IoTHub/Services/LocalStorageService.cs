using System;
using System.IO;
using Windows.Storage;
using HouseOfTheFuture.IoTHub.Entities;
using Newtonsoft.Json;

namespace HouseOfTheFuture.IoTHub.Services
{
    interface ILocalStorageService
    {
        DeviceIdentifier? CheckForExistingDeviceIdentifier();
        void PersistDeviceIdentifier(DeviceIdentifier deviceIdentifier);
    }

    class LocalStorageService
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
