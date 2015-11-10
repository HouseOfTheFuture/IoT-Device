using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using HouseOfTheFuture.RF24Library;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using Windows.Storage;
using System.Diagnostics;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading;
using Windows.Devices.Enumeration;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace HouseOfTheFuture.RF24Communicator
{
    public sealed class StartupTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
           // RunRF24CommunicationModule();
            RunSerialCommunicationModule();
        }

        private async void RunSerialCommunicationModule()
        {
            try {
                var module = new SerialCommunicationModule();
                var results = module.ListAvailablePorts().Result;
                module.SelectDevice(results.First());
                while(true)
                {
                    Task.Delay(1000).Wait();
                }
            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            
        }

        public void RunRF24CommunicationModule()
        {
            try
            {
                var module = new NRF24L01Plus();
                var address = StringToByteArray("E8E8F0F0E1");
                var myAddress = StringToByteArray("E8E8F0F0E2");
                byte channel = 1;


                module.Initialize(Windows.Devices.Spi.SpiMode.Mode0, 24, 22, 13);
                module.SetAddress(AddressSlot.Zero, myAddress);
                module.Configure(myAddress, channel, NRFDataRate.DR2Mbps);
                var status = module.GetStatus();
                //module.SendTo(address, new byte[] { 1, 2, 3 }, Acknowledge.No);
                module.OnTransmitFailed += Module_OnTransmitFailed;
                module.OnDataReceived += Module_OnDataReceived;
                module.OnTransmitSuccess += Module_OnTransmitSuccess;
                module.Enable();
                var i = 0;
                while (true)
                {
                    i++;
                    module.HandleInterrupt(0, 0, DateTime.Now);
                    Debug.WriteLine(i);
                    Task.Delay(1).Wait();
                }

            }
            catch (Exception e)
            {

                throw;
            }
        }

        private void Module_OnTransmitSuccess()
        {
            Debug.WriteLine("Transmit success");
        }

        private void Module_OnTransmitFailed()
        {
            Debug.WriteLine("Transmit failed");
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void Module_OnDataReceived(byte[] data)
        {
            Debug.WriteLine("Data received:" + string.Join(";", data.Select(x => x.ToString())));
        }
    }
}
