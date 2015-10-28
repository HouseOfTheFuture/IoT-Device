﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using HouseOfTheFuture.RF24Library;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace HouseOfTheFuture.RF24Communicator
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                var module = new NRF24L01Plus();
                module.Initialize(Windows.Devices.Spi.SpiMode.Mode0, 24, 22, 13);
            }
            catch (Exception e)
            {

                throw;
            }

        }
    }
}
