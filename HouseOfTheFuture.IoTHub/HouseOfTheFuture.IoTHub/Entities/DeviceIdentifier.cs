using System;

namespace HouseOfTheFuture.IoTHub.Entities
{
    public struct DeviceIdentifier
    {
        public Guid Identifier { get; private set; }
        
        public DeviceIdentifier(Guid identifier)
        {
            Identifier = identifier;
        }
        
        public DeviceIdentifier(string identifier)
        {
            Identifier = Guid.Parse(identifier);
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }
    }
}