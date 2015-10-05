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
        public MainPage()
        {
            InitializeComponent();
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
            using (var socket = new DatagramSocket())
            {
                socket.MessageReceived += Socket_MessageReceived;
                await socket.BindServiceNameAsync("5321", hostname.IPInformation.NetworkAdapter);
                Debug.WriteLine("CONNECTED");
                await socket.ConnectAsync(new HostName("255.255.255.255"), "5321");
                var stream = socket.OutputStream;


                var writer = new DataWriter(stream);

                writer.WriteString("TICK");

                await writer.StoreAsync();
            }
        }

        private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var stream = sender.OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteString("TACK");

            writer.StoreAsync();
        }

        private async Task SendTack()
        {
            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(new HostName("255.255.255.255"), "5321");
                var stream = socket.OutputStream;

                var writer = new DataWriter(stream);

                writer.WriteString("TACK");

                await writer.StoreAsync();
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
