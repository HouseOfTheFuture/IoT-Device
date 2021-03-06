﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.9.7.0
// Changes may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TickTack.Models;

namespace TickTack.Models
{
    public partial class GetDeviceResponse
    {
        private DeviceDto _device;
        
        /// <summary>
        /// Optional.
        /// </summary>
        public DeviceDto Device
        {
            get { return this._device; }
            set { this._device = value; }
        }
        
        /// <summary>
        /// Initializes a new instance of the GetDeviceResponse class.
        /// </summary>
        public GetDeviceResponse()
        {
        }
        
        /// <summary>
        /// Deserialize the object
        /// </summary>
        public virtual void DeserializeJson(JToken inputObject)
        {
            if (inputObject != null && inputObject.Type != JTokenType.Null)
            {
                JToken deviceValue = inputObject["device"];
                if (deviceValue != null && deviceValue.Type != JTokenType.Null)
                {
                    DeviceDto deviceDto = new DeviceDto();
                    deviceDto.DeserializeJson(deviceValue);
                    this.Device = deviceDto;
                }
            }
        }
    }
}
