using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HouseOfTheFuture.IoTHub.Entities;
using HouseOfTheFuture.IoTHub.Helpers;
using HouseOfTheFuture.IoTHub.Services;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using TickTack;
using TickTack.Models;

namespace HouseOfTheFuture.IoTHub
{
    public sealed partial class MainPage 
        : Page
    {            
        private readonly StringBuilder _log;        
        private Task<DatagramSocket>[] _sockets;

        private readonly ILocalStorageService _localStorageService;
        private readonly FakeUsageReporter _fakeUsageReporter;

        public MainPage()
            : this(new LocalStorageService(), new FakeUsageReporter())
        {
        }

        public MainPage(ILocalStorageService localStorageService, FakeUsageReporter fakeUsageReporter)
        {
            InitializeComponent();

            _log = new StringBuilder();

            _localStorageService = localStorageService;
            _fakeUsageReporter = fakeUsageReporter;
        }            

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await InitializeDeviceIdentifierAsync();

            ListenForBroadcastFromMobileApp();
        }

        private async Task InitializeDeviceIdentifierAsync()
        {
            var existingDeviceIdentifier = _localStorageService.CheckForExistingDeviceIdentifier();
            var deviceRegistration = await RegisterDeviceAsync(existingDeviceIdentifier);

            DeviceRegistration.Current = deviceRegistration;

            _localStorageService.PersistDeviceIdentifier(deviceRegistration.DeviceIdentifier);

            _fakeUsageReporter.InitializeIoTHubClient();
        }

        private void ListenForBroadcastFromMobileApp()
        {
            var adapters = NetworkInformation.GetHostNames()
                .Where(x => x.IPInformation != null
                            && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                                || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                .Select(x => x.IPInformation.NetworkAdapter)
                .Distinct(new NetworkAdapterComparer())
                .ToArray(); // ethernet

            _sockets = adapters.Select(ListenForTick).ToArray();
        }

        private async Task<DatagramSocket> ListenForTick(NetworkAdapter hostname)
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += OnMessageReceived;

            await socket.BindServiceNameAsync(Settings.RemoteServiceName, hostname);
            socket.JoinMulticastGroup(new HostName(Settings.MulticastAddress));

            Log($"Listening on {hostname.NetworkAdapterId}:{Settings.RemoteServiceName}");

            return socket;
        }

        private static async Task<DeviceRegistration> RegisterDeviceAsync(DeviceIdentifier? deviceIdentifier)
        {
            using (var client = new HouseOfTheFutureApiHost())
            {
                var response = await client.IotRegister
                    .PostWithOperationResponseAsync(new RegisterIotDeviceRequest
                    {
                        CurrentDeviceId = deviceIdentifier?.ToString()
                    });
                
                return new DeviceRegistration
                {
                    DeviceIdentifier = new DeviceIdentifier(response.Body.DeviceId),
                    HubDeviceKey = response.Body.HubDeviceKey,
                    IsConfigured = response.Body.IsConfigured ?? false
                };
            }            
        }                

        private async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Log($"TICK Received FROM {args.RemoteAddress.DisplayName}");

            using (var reader = args.GetDataReader())
            {
                var length = reader.UnconsumedBufferLength;
                var data = reader.ReadString(length);

                if (data.StartsWith(Protocol.TickTack.ReceiveDeviceInfoCommand))
                {
                    var remoteAddress = args.RemoteAddress;

                    await SendIdentifierToDevice(remoteAddress);                    
                }
            }                       
        }

        private async Task SendIdentifierToDevice(HostName remoteAddress)
        {
            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(remoteAddress, Settings.RemoteServiceName);

                using (var dataStream = socket.OutputStream.AsStreamForWrite())
                {                    
                    var identifier = DeviceRegistration.Current.DeviceIdentifier.ToString();

                    var data = Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", 
                        Protocol.TickTack.SendDeviceInfoCommand, identifier));

                    dataStream.Write(data, 0, data.Length);

                    await dataStream.FlushAsync();

                    Log($"TACK sent to {remoteAddress} - device identifier: {identifier}");
                }
            }
        }

        private void MainPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (var socket in _sockets)
            {
                socket.Result.Dispose();
            }
        }

        private async Task Log(string format)
        {
            if (Dispatcher.HasThreadAccess)
            {
                _log.AppendLine(string.Format("{0} - {1}", DateTime.Now, format));
                textBlock.Text = _log.ToString();
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Log(format));
            }
        }

        private async void FakeTickForced(object sender, RoutedEventArgs e)
        {
            var clickedButton = (Button) sender;
            var content = clickedButton.Content.ToString();

            if (content.Contains(MeterType.Electricity.ToString()))
            {
                await _fakeUsageReporter.ReportFakeUsageForAsync(MeterType.Electricity);
            }
            else if (content.Contains(MeterType.Water.ToString()))
            {
                await _fakeUsageReporter.ReportFakeUsageForAsync(MeterType.Water);
            }
            else if (content.Contains(MeterType.Gas.ToString()))
            {
                await _fakeUsageReporter.ReportFakeUsageForAsync(MeterType.Gas);
            }

            Log($"{content} reported.");
        }
    }

    public class FakeUsageReporter
    {
        private Dictionary<MeterType, Guid> _fakeSensorIdentifiers;

        private DeviceClient _iotHubClient;

        public FakeUsageReporter()
        {
            SetupFakeSensorIdentifiers();            
        }

        public void InitializeIoTHubClient()
        {            
            _iotHubClient = DeviceClient.Create(Settings.IoTHubEndpoint,
                new DeviceAuthenticationWithRegistrySymmetricKey(
                    DeviceRegistration.Current.DeviceIdentifier.ToString(),
                    DeviceRegistration.Current.HubDeviceKey), TransportType.Http1);
        }

        private void SetupFakeSensorIdentifiers()
        {
            _fakeSensorIdentifiers = new Dictionary<MeterType, Guid>
            {
                { MeterType.Electricity, Guid.NewGuid() },
                { MeterType.Water, Guid.NewGuid() },
                { MeterType.Gas, Guid.NewGuid() }
            };
        }

        public async Task ReportFakeUsageForAsync(MeterType meterType)
        {
            var data = new PostRequest               
            {
                HubId = DeviceRegistration.Current.DeviceIdentifier.ToString(),
                Value = 1,
                SensorId = _fakeSensorIdentifiers[meterType].ToString(),
                Timestamp = DateTime.UtcNow
            };

            var messageString = JsonConvert.SerializeObject(data);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _iotHubClient.SendEventAsync(message);
        }
    }

    public enum MeterType
    {
        Electricity,
        Water,
        Gas
    }
}