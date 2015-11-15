using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace HouseOfTheFuture.RF24Communicator
{
    class SerialCommunicationModule
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private SerialDevice serialPort = null;

        private InputStreamReader _streamReader;

        private string _text;
        public string Text { get { return _text; }
                set {
                Debug.WriteLine("text='"+ value +"'");
                _text = value;
                }
            }

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;

        public SerialCommunicationModule()
        {
            listOfDevices = new ObservableCollection<DeviceInformation>();
        }

        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
        public async Task<IEnumerable<DeviceInformation>> ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                return listOfDevices;
                
            }
            catch (Exception ex)
            {
                Text = ex.Message;
                return Enumerable.Empty<DeviceInformation>();
            }
        }

        /// <summary>
        /// comPortInput_Click: Action to take when 'Connect' button is clicked
        /// - Get the selected device index and use Id to create the SerialDevice object
        /// - Configure default settings for the serial port
        /// - Create the ReadCancellationTokenSource token
        /// - Start listening on the serial port input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void SelectDevice(DeviceInformation device)
        {
            try
            {
                serialPort = await SerialDevice.FromIdAsync(device.Id);

                // Configure serial settings
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                _streamReader = new InputStreamReader(serialPort.InputStream, 128);
                _streamReader.OnLineRead += OnLineRead;
                _streamReader.StartReading();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OnLineRead(object sender, string e)
        {
            Debug.WriteLine(e);
        }
    }
}
