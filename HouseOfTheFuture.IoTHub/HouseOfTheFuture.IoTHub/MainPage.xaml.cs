using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SocketError = System.Net.Sockets.SocketError;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HouseOfTheFuture.IoTHub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string MulticastAddress = "239.255.42.99";
        private const string RemoteServiceName = "5321";
        private StringBuilder _log;
        private HostName[] _adapters;

        public MainPage()
        {
            InitializeComponent();
            _log = new StringBuilder();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _adapters = NetworkInformation.GetHostNames()
                .Where(x => x.IPInformation != null
                            && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                                || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6) && x.Type == HostNameType.Ipv4).ToArray(); // ethernet
            foreach (var hostName in _adapters)
            {
                ListenForTick(hostName);
            }
        }

        private async Task ListenForTick(HostName hostname)
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
            await socket.BindServiceNameAsync(RemoteServiceName, hostname.IPInformation.NetworkAdapter);
            socket.JoinMulticastGroup(new HostName(MulticastAddress));
            Log(string.Format("Listening on {0}:{1}", hostname.DisplayName, RemoteServiceName));
            
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
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Log(format);
                });
            }
        }

        private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Log(string.Format("TICK Received FROM {0}", args.RemoteAddress.DisplayName));
        }

        private async Task SendTack()
        {
            foreach (var hostName in _adapters)
            {
                using (var socket = new DatagramSocket())
                {
                    await socket.BindServiceNameAsync("", hostName.IPInformation.NetworkAdapter);
                    socket.JoinMulticastGroup(new HostName(MulticastAddress));
                    IOutputStream outputStream = await socket.GetOutputStreamAsync(new HostName(MulticastAddress), RemoteServiceName);
                    byte[] buffer = Encoding.UTF8.GetBytes("Protocol message");
                    await outputStream.WriteAsync(buffer.AsBuffer());
                    await outputStream.FlushAsync();

                    Log(string.Format("TACK Broadcasted to {0}:{1}", MulticastAddress, RemoteServiceName));
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SendTack();
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
