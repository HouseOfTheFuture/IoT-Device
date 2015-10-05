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
        private StringBuilder _log;

        public MainPage()
        {
            InitializeComponent();
            _log = new StringBuilder();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ListenOnAllIps();
        }

        private async Task ListenOnAllIps()
        {
            foreach (var hostname in NetworkInformation.GetHostNames()
                .Where(x => x.IPInformation != null
                            && (x.IPInformation.NetworkAdapter.IanaInterfaceType == 71 // wifi
                                || x.IPInformation.NetworkAdapter.IanaInterfaceType == 6) && x.Type == HostNameType.Ipv4)) // ethernet
            {
                ListenForTick(hostname);
            }
        }

        private async Task ListenForTick(HostName hostname)
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
            await socket.BindServiceNameAsync("5321");
            // TODO: Multicast: socket.JoinMulticastGroup(new HostName("255.255.255.255"));
            Log(string.Format("Listening on {0}:{1}", hostname.DisplayName, 5321));
            
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

        private async Task SendTack(string hostname)
        {
            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(new HostName(hostname), "5321");
                var stream = socket.OutputStream;

                var writer = new DataWriter(stream);

                writer.WriteString("TACK");

                await writer.StoreAsync();

                Log(string.Format("TACK Broadcasted to {0}:5321", hostname));
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SendTack("255.255.255.255");
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
