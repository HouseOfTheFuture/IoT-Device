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
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

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

                Listen();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        ///// <summary>
        ///// sendTextButton_Click: Action to take when 'WRITE' button is clicked
        ///// - Create a DataWriter object with the OutputStream of the SerialDevice
        ///// - Create an async task that performs the write operation
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void sendTextButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (serialPort != null)
        //        {
        //            // Create the DataWriter object and attach to OutputStream
        //            dataWriteObject = new DataWriter(serialPort.OutputStream);

        //            //Launch the WriteAsync task to perform the write
        //            await WriteAsync();
        //        }
        //        else
        //        {
        //            status.Text = "Select a device and connect";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status.Text = "sendTextButton_Click: " + ex.Message;
        //    }
        //    finally
        //    {
        //        // Cleanup once complete
        //        if (dataWriteObject != null)
        //        {
        //            dataWriteObject.DetachStream();
        //            dataWriteObject = null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        ///// </summary>
        ///// <returns></returns>
        //private async Task WriteAsync()
        //{
        //    Task<UInt32> storeAsyncTask;

        //    if (sendText.Text.Length != 0)
        //    {
        //        // Load the text from the sendText input text box to the dataWriter object
        //        dataWriteObject.WriteString(sendText.Text);

        //        // Launch an async task to complete the write operation
        //        storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

        //        UInt32 bytesWritten = await storeAsyncTask;
        //        if (bytesWritten > 0)
        //        {
        //            status.Text = sendText.Text + ", ";
        //            status.Text += "bytes written successfully!";
        //        }
        //        sendText.Text = "";
        //    }
        //    else
        //    {
        //        status.Text = "Enter the text you want to write and then click on 'WRITE'";
        //    }
        //}

        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    CloseDevice();
                }
                else
                {
                    Text = ex.Message;
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 128;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            try {
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    byte[] read = new byte[bytesRead];
                    dataReaderObject.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    dataReaderObject.ReadBytes(read);
                    Text = Encoding.UTF8.GetString(read);
                }
            }catch(Exception ex)
            {
                Text = ex.Message;
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        public void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            
            listOfDevices.Clear();
        }

        /// <summary>
        /// closeDevice_Click: Action to take when 'Disconnect and Refresh List' is clicked on
        /// - Cancel all read operations
        /// - Close and dispose the SerialDevice object
        /// - Enumerate connected devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void closeDevice()
        {
            try
            {
                Text = "";
                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                Text = ex.Message;
            }
        }
    }
}
