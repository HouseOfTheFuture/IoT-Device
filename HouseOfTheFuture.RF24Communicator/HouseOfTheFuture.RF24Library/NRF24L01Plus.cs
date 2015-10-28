using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace HouseOfTheFuture.RF24Library
{
    public class NRF24L01Plus
    {
        #region Delegates

        public delegate void EventHandler();

        public delegate void OnDataRecievedHandler(byte[] data);

        public delegate void OnInterruptHandler(Status status);

        #endregion

        private byte[] _slot0Address;
        private GpioPin _cePin; // private OutputPort _cePin;
        private bool _initialized;
        private GpioPin _irqPin; //private InterruptPort _irqPin;
        private SpiDevice _spiDevice;
        private bool _enabled;
        private readonly ManualResetEvent _transmitSuccessFlag;
        private readonly ManualResetEvent _transmitFailedFlag;

        /// <summary>
        ///   Gets a value indicating whether module is enabled (RX or TX mode).
        /// </summary>
        public bool IsEnabled
        {
            get {
                return _cePin.Read() == GpioPinValue.High; // return _cePin.Read(); 
            }
        }

        public NRF24L01Plus()
        {
            _transmitSuccessFlag = new ManualResetEvent(false);
            _transmitFailedFlag = new ManualResetEvent(false);
        }

        /// <summary>
        ///   Enables the module
        /// </summary>
        public void Enable()
        {
            _enabled = true;
            SetEnabled();
        }

        /// <summary>
        ///   Disables the module
        /// </summary>
        public void Disable()
        {
            _enabled = false;
            SetDisabled();
        }

        /// <summary>
        ///   Initializes SPI connection and control pins
        /// </summary>
        public void Initialize(SpiMode spiMode, int chipSelectPin, int chipEnablePin, int interruptPin)
        {
            var gpio = GpioController.GetDefault();
            // Chip Select : Active Low
            // Clock : Active High, Data clocked in on rising edge
            _spiDevice = InitSpi(chipSelectPin, spiMode).Result;

            _irqPin = gpio.OpenPin(interruptPin);
            _irqPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            
            // Initialize IRQ Port
            //_irqPin = new InterruptPort(interruptPin, false, Port.ResistorMode.PullUp,
            //                            Port.InterruptMode.InterruptEdgeLow);
            //_irqPin.OnInterrupt += HandleInterrupt;

            _cePin = gpio.OpenPin(chipEnablePin);
            // Initialize Chip Enable Port
            _cePin.SetDriveMode(GpioPinDriveMode.Output);

            // Module reset time
            var task = Task.Delay(100);
            task.Start();
            task.Wait();

            _initialized = true;
        }

        /* Initialize the SPI bus */
        private async Task<SpiDevice> InitSpi(int chipSelectPin, SpiMode mode)
        {
            try
            {
                //new SPI(new SPI.Configuration(chipSelectPin, false, 0, 0, false, true, 2000, spi));
                //public Configuration(Cpu.Pin ChipSelect_Port, bool ChipSelect_ActiveState, uint ChipSelect_SetupTime, uint ChipSelect_HoldTime, bool Clock_IdleState, bool Clock_Edge, uint Clock_RateKHz, SPI.SPI_module SPI_mod);
                var settings = new SpiConnectionSettings(chipSelectPin); /* Create SPI initialization settings                               */
                settings.ClockFrequency = 20000000;                             /* Datasheet specifies maximum SPI clock frequency of 10MHz         */
                settings.Mode = mode;                                  /* The display expects an idle-high clock polarity, we use Mode3
                                                                         * to set the clock polarity and phase to: CPOL = 1, CPHA = 1
                                                                         */

                string spiAqs = SpiDevice.GetDeviceSelector();       /* Find the selector string for the SPI bus controller          */
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);         /* Find the SPI bus controller device with our selector string  */
                return await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */

            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private void SetEnabled()
        {
   //         _irqPin.EnableInterrupt();
               _cePin.Write(GpioPinValue.High);
        }

        private void SetDisabled()
        {
            _cePin.Write(GpioPinValue.Low);
            //_irqPin.DisableInterrupt();
        }

        /// <summary>
        /// Configure the module basic settings. Module needs to be initiaized.
        /// </summary>
        /// <param name="address">RF address (3-5 bytes). The width of this address determins the width of all addresses used for sending/receiving.</param>
        /// <param name="channel">RF channel (0-127)</param>
        public void Configure(byte[] address, byte channel)
        {
            Configure(address, channel, NRFDataRate.DR2Mbps);
        }

        /// <summary>
        /// Configure the module basic settings. Module needs to be initiaized.
        /// </summary>
        /// <param name="address">RF address (3-5 bytes). The width of this address determins the width of all addresses used for sending/receiving.</param>
        /// <param name="channel">RF channel (0-127)</param>
        /// <param name="dataRate">Data Rate to use</param>
        public void Configure(byte[] address, byte channel, NRFDataRate dataRate)
        {
            CheckIsInitialized();
            AddressWidth.Check(address);

            // Set radio channel
            Execute(Commands.W_REGISTER, Registers.RF_CH,
                    new[]
                        {
                            (byte) (channel & 0x7F) // channel is 7 bits
                        });

            // Set Data rate
            var regValue = Execute(Commands.R_REGISTER, Registers.RF_SETUP, new byte[1])[1];

            switch (dataRate)
            {
                case NRFDataRate.DR1Mbps:
                    regValue &= (byte)~(1 << Bits.RF_DR_LOW);  // 0
                    regValue &= (byte)~(1 << Bits.RF_DR_HIGH); // 0
                    break;

                case NRFDataRate.DR2Mbps:
                    regValue &= (byte)~(1 << Bits.RF_DR_LOW);  // 0
                    regValue |= (byte)(1 << Bits.RF_DR_HIGH);  // 1
                    break;

                case NRFDataRate.DR250kbps:
                    regValue |= (byte)(1 << Bits.RF_DR_LOW);   // 1
                    regValue &= (byte)~(1 << Bits.RF_DR_HIGH); // 0
                    break;

                default:
                    throw new ArgumentOutOfRangeException("dataRate");
            }

            Execute(Commands.W_REGISTER, Registers.RF_SETUP, new[] { regValue });

            // Enable dynamic payload length
            Execute(Commands.W_REGISTER, Registers.FEATURE,
                    new[]
                        {
                            (byte) (1 << Bits.EN_DPL)
                        });

            // Set auto-ack
            Execute(Commands.W_REGISTER, Registers.EN_AA,
                    new[]
                        {
                            (byte) (1 << Bits.ENAA_P0 |
                                    1 << Bits.ENAA_P1)
                        });

            // Set dynamic payload length for pipes
            Execute(Commands.W_REGISTER, Registers.DYNPD,
                    new[]
                        {
                            (byte) (1 << Bits.DPL_P0 |
                                    1 << Bits.DPL_P1)
                        });

            // Flush RX FIFO
            Execute(Commands.FLUSH_RX, 0x00, new byte[0]);

            // Flush TX FIFO
            Execute(Commands.FLUSH_TX, 0x00, new byte[0]);

            // Clear IRQ Masks
            Execute(Commands.W_REGISTER, Registers.STATUS,
                    new[]
                        {
                            (byte) (1 << Bits.MASK_RX_DR |
                                    1 << Bits.MASK_TX_DS |
                                    1 << Bits.MAX_RT)
                        });

            // Set default address
            Execute(Commands.W_REGISTER, Registers.SETUP_AW,
                    new[]
                        {
                            AddressWidth.Get(address)
                        });

            // Set module address
            _slot0Address = address;
            Execute(Commands.W_REGISTER, (byte)AddressSlot.Zero, address);

            // Set retransmission values
            Execute(Commands.W_REGISTER, Registers.SETUP_RETR,
                    new[]
                        {
                            (byte) (0x0F << Bits.ARD |
                                    0x0F << Bits.ARC)
                        });

            // Setup, CRC enabled, Power Up, PRX
            SetReceiveMode();
        }

        /// <summary>
        /// Set one of 6 available module addresses
        /// </summary>
        public void SetAddress(AddressSlot slot, byte[] address)
        {
            CheckIsInitialized();
            AddressWidth.Check(address);
            Execute(Commands.W_REGISTER, (byte)slot, address);

            if (slot == AddressSlot.Zero)
            {
                _slot0Address = address;
            }
        }

        /// <summary>
        /// Read 1 of 6 available module addresses
        /// </summary>
        public byte[] GetAddress(AddressSlot slot, int width)
        {
            CheckIsInitialized();
            AddressWidth.Check(width);
            var read = Execute(Commands.R_REGISTER, (byte)slot, new byte[width]);
            var result = new byte[read.Length - 1];
            Array.Copy(read, 1, result, 0, result.Length);
            return result;
        }

        /// <summary>
        ///   Executes a command in NRF24L01+ (for details see module datasheet)
        /// </summary>
        /// <param name = "command">Command</param>
        /// <param name = "addres">Register to write to</param>
        /// <param name = "data">Data to write</param>
        /// <returns>Response byte array. First byte is the status register</returns>
        public byte[] Execute(byte command, byte addres, byte[] data)
        {
            CheckIsInitialized();

            // This command requires module to be in power down or standby mode
            if (command == Commands.W_REGISTER)
                SetDisabled();

            // Create SPI Buffers with Size of Data + 1 (For Command)
            var writeBuffer = new byte[data.Length + 1];
            var readBuffer = new byte[data.Length + 1];

            // Add command and adres to SPI buffer
            writeBuffer[0] = (byte)(command | addres);

            // Add data to SPI buffer
            Array.Copy(data, 0, writeBuffer, 1, data.Length);

            // Do SPI Read/Write
            _spiDevice.Read(readBuffer);
            _spiDevice.Write(writeBuffer);
            //_spiPort.WriteRead(writeBuffer, readBuffer);

            // Enable module back if it was disabled
            if (command == Commands.W_REGISTER && _enabled)
                SetEnabled();

            // Return ReadBuffer
            return readBuffer;
        }

        /// <summary>
        ///   Gets module basic status information
        /// </summary>
        public Status GetStatus()
        {
            CheckIsInitialized();

            var readBuffer = new byte[1];
            _spiDevice.Write(new byte[] { Commands.NOP });
            _spiDevice.Read(readBuffer);
//            _spiPort.WriteRead(new[] { Commands.NOP }, readBuffer);

            return new Status(readBuffer[0]);
        }

        /// <summary>
        ///   Reads the current rf channel value set in module
        /// </summary>
        /// <returns></returns>
        public byte GetChannel()
        {
            CheckIsInitialized();

            var result = Execute(Commands.R_REGISTER, Registers.RF_CH, new byte[1]);
            return (byte)(result[1] & 0x7F);
        }

        /// <summary>
        ///   Gets the module radio frequency [MHz]
        /// </summary>
        /// <returns>Frequency in MHz</returns>
        public int GetFrequency()
        {
            return 2400 + GetChannel();
        }

        /// <summary>
        ///   Sets the rf channel value used by all data pipes
        /// </summary>
        /// <param name="channel">7 bit channel value</param>
        public void SetChannel(byte channel)
        {
            CheckIsInitialized();

            var writeBuffer = new[] { (byte)(channel & 0x7F) };
            Execute(Commands.W_REGISTER, Registers.RF_CH, writeBuffer);
        }

        /// <summary>
        ///   Send <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a non blocking method.
        /// </summary>
        public void SendTo(byte[] address, byte[] bytes, Acknowledge acknowledge = Acknowledge.Yes)
        {
            // Chip enable low
            SetDisabled();

            // Setup PTX (Primary TX)
            SetTransmitMode();

            // Write transmit adres to TX_ADDR register. 
            Execute(Commands.W_REGISTER, Registers.TX_ADDR, address);

            // Write transmit adres to RX_ADDRESS_P0 (Pipe0) (For Auto ACK)
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, address);

            // Send payload
            Execute(acknowledge == Acknowledge.Yes ? Commands.W_TX_PAYLOAD : Commands.W_TX_PAYLOAD_NO_ACK, 0x00, bytes);

            // Pulse for CE -> starts the transmission.
            SetEnabled();
        }

        /// <summary>
        ///   Sends <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a blocking method that returns true if data was received by the recipient or false if timeout occured.
        /// </summary>
        public bool SendTo(byte[] address, byte[] bytes, int timeout)
        {
            var startTime = DateTime.Now;

            while (true)
            {
                _transmitSuccessFlag.Reset();
                _transmitFailedFlag.Reset();

                SendTo(address, bytes);

                if (WaitHandle.WaitAny(new[] { _transmitSuccessFlag, _transmitFailedFlag }, 200) == 0)
                    return true;

                if (DateTime.Now.CompareTo(startTime.AddMilliseconds(timeout)) > 0)
                    return false;

                Debug.WriteLine("Retransmitting packet...");
            }
        }

        private void HandleInterrupt(uint data1, uint data2, DateTime dateTime)
        {
            if (!_initialized)
                return;

            if (!_enabled)
            {
                // Flush RX FIFO
                Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);
                return;
            }

            // Disable RX/TX
            SetDisabled();

            // Set PRX
            SetReceiveMode();

            // there are 3 rx pipes in rf module so 3 arrays should be enough to store incoming data
            // sometimes though more than 3 data packets are received somehow
            var payloads = new byte[6][];

            var status = GetStatus();
            byte payloadCount = 0;
            var payloadCorrupted = false;

            OnInterrupt(status);

            if (status.DataReady)
            {
                while (!status.RxEmpty)
                {
                    // Read payload size
                    var payloadLength = Execute(Commands.R_RX_PL_WID, 0x00, new byte[1]);

                    // this indicates corrupted data
                    if (payloadLength[1] > 32)
                    {
                        payloadCorrupted = true;

                        // Flush anything that remains in buffer
                        Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                    }
                    else
                    {
                        if (payloadCount >= payloads.Length)
                        {
                            Debug.WriteLine("Unexpected payloadCount value = " + payloadCount);
                            Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                        }
                        else
                        {
                            // Read payload data
                            payloads[payloadCount] = Execute(Commands.R_RX_PAYLOAD, 0x00, new byte[payloadLength[1]]);
                            payloadCount++;
                        }
                    }

                    // Clear RX_DR bit 
                    var result = Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.RX_DR) });
                    status.Update(result[0]);
                }
            }

            if (status.ResendLimitReached)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);

                // Clear MAX_RT bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.MAX_RT) });
            }

            if (status.TxFull)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);
            }

            if (status.DataSent)
            {
                // Clear TX_DS bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.TX_DS) });
            }

            // Enable RX
            SetEnabled();

            if (payloadCorrupted)
            {
                Debug.WriteLine("Corrupted data received");
            }
            else if (payloadCount > 0)
            {
                if (payloadCount > payloads.Length)
                    Debug.WriteLine("Unexpected payloadCount value = " + payloadCount);

                for (var i = 0; i < System.Math.Min(payloadCount, payloads.Length); i++)
                {
                    var payload = payloads[i];
                    var payloadWithoutCommand = new byte[payload.Length - 1];
                    Array.Copy(payload, 1, payloadWithoutCommand, 0, payload.Length - 1);
                    OnDataReceived(payloadWithoutCommand);
                }
            }
            else if (status.DataSent)
            {
                _transmitSuccessFlag.Set();
                OnTransmitSuccess();
            }
            else
            {
                _transmitFailedFlag.Set();
                OnTransmitFailed();
            }
        }


        private void SetTransmitMode()
        {
            Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (byte) (1 << Bits.PWR_UP |
                                    1 << Bits.CRCO)
                        });
        }

        private void SetReceiveMode()
        {
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, _slot0Address);

            Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (byte) (1 << Bits.PWR_UP |
                                    1 << Bits.CRCO |
                                    1 << Bits.PRIM_RX)
                        });
        }

        private void CheckIsInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Initialize method needs to be called before this call");
            }
        }

        /// <summary>
        ///   Called on every IRQ interrupt
        /// </summary>
        public event OnInterruptHandler OnInterrupt = delegate { };

        /// <summary>
        ///   Occurs when data packet has been received
        /// </summary>
        public event OnDataRecievedHandler OnDataReceived = delegate { };

        /// <summary>
        ///   Occurs when ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitSuccess = delegate { };

        /// <summary>
        ///   Occurs when no ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitFailed = delegate { };
    }
}
