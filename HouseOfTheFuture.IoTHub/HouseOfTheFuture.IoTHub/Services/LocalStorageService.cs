using System;
using HouseOfTheFuture.IoTHub.Entities;

namespace HouseOfTheFuture.IoTHub.Services
{
    interface ILocalStorageService
    {
        DeviceIdentifier? CheckForExistingDeviceIdentifier();
        void PersistDeviceIdentifier();
    }

    class LocalStorageService
        : ILocalStorageService
    {
        public DeviceIdentifier? CheckForExistingDeviceIdentifier()
        {
            // todo: Retrieve existing device identifier from the Pi if present. 

            return new DeviceIdentifier(Guid.NewGuid());
        }

        public void PersistDeviceIdentifier()
        {
            // todo: Store the device identifier to the Pi's local storage. 

            throw new NotImplementedException();
        }
    }    
}