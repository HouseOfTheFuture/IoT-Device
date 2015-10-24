using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HouseOfTheFuture.IoTHub.Entities;
using HouseOfTheFuture.IoTHub.Helpers;
using HouseOfTheFuture.IoTHub.Models;
using HouseOfTheFuture.IoTHub.Services;

namespace HouseOfTheFuture.IoTHub
{
    public sealed partial class MainPage : Page
    {
        private const string MulticastAddress = "239.255.42.99";
        private const string RemoteServiceName = "5321";

        private readonly StringBuilder _log;

        private NetworkAdapter[] _adapters;
        private Task<DatagramSocket>[] _sockets;

        private readonly ILocalStorageService _localStorageService;

        public MainPage()
        {
            InitializeComponent();

            _log = new StringBuilder();

            _localStorageService = new LocalStorageService();
        }                

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ListenForBroadcastFromMobileApp();

            var existingDeviceIdentifier = _localStorageService.CheckForExistingDeviceIdentifier();
            var deviceRegistration = await RegisterDeviceAsync(existingDeviceIdentifier);

            if (!existingDeviceIdentifier.HasValue)
            {
                _localStorageService.PersistDeviceIdentifier();
            }
            
            BroadcastDeviceIdentifier(deviceRegistration.DeviceIdentifier);                        
        }        

        private void ListenForBroadcastFromMobileApp()
        {
            _adapters = NetworkInformation.GetHostNames()
                .Where(x => x.IPInformation != null
                            && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                                || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                .Select(x => x.IPInformation.NetworkAdapter)
                .Distinct(new NetworkAdapterComparer())
                .ToArray(); // ethernet

            _sockets = _adapters.Select(ListenForTick).ToArray();
        }

        private async Task<DeviceRegistration> RegisterDeviceAsync(DeviceIdentifier? deviceIdentifier)
        {
            using (var client = new HouseOfTheFutureApiHost())
            {
                var response = await client.IotRegister
                    .PostWithOperationResponseAsync(new RegisterIotDeviceRequest
                    {
                        CurrentDeviceId = deviceIdentifier?.ToString()
                    });
                
                var deviceId = response.Body.DeviceId;

                return new DeviceRegistration {  DeviceIdentifier = new DeviceIdentifier(deviceId) };
            }            
        }

        private async Task<DatagramSocket> ListenForTick(NetworkAdapter hostname)
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;

            await socket.BindServiceNameAsync(RemoteServiceName, hostname);
            socket.JoinMulticastGroup(new HostName(MulticastAddress));

            Log(string.Format("Listening on {0}:{1}", hostname.NetworkAdapterId, RemoteServiceName));

            return socket;
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

        private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Log(string.Format("TICK Received FROM {0}", args.RemoteAddress.DisplayName));
        }

        private async Task BroadcastDeviceIdentifier(DeviceIdentifier deviceIdentifier)
        {
            foreach (var hostName in _adapters)
            {
                using (var socket = new DatagramSocket())
                {
                    await socket.BindServiceNameAsync("", hostName);

                    socket.JoinMulticastGroup(new HostName(MulticastAddress));

                    IOutputStream outputStream = await socket.GetOutputStreamAsync(new HostName(MulticastAddress), RemoteServiceName);
                    byte[] buffer = Encoding.UTF8.GetBytes(deviceIdentifier.ToString());

                    await outputStream.WriteAsync(buffer.AsBuffer());
                    await outputStream.FlushAsync();

                    Log(string.Format("TACK Broadcasted to {0}:{1} - Device Identifier: {2}", 
                        MulticastAddress, RemoteServiceName, deviceIdentifier.ToString()));
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            BroadcastDeviceIdentifier(new DeviceIdentifier(Guid.NewGuid()));
        }

        private void MainPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (var socket in _sockets)
            {
                socket.Result.Dispose();
            }
        }
    }

    class NetworkInterface
    {
        public static string[] IpAddresses
        {
            get
            {
                return NetworkInformation.GetHostNames()
                    .Where(x =>x.IPInformation != null 
                    && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                    || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6))// ethernet
                    .Select(x => x.DisplayName).ToArray(); 
            }
        }
    }
}