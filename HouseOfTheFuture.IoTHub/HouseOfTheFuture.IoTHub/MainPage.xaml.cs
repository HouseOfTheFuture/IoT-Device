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
            using (NetworkInterface ni = new NetworkInterface())
            {
                Debug.WriteLine("Clicked!");
                ni.Connect(new HostName("255.255.255.255"), "5321");
                string cmd = "Hello there\r";

                ni.SendMessage(cmd);
            }
        }
    }
    class NetworkInterface : IDisposable
    {
        private readonly DatagramSocket _socket;

        public NetworkInterface()
        {
            _socket = new DatagramSocket();
        }

        public async void Connect(HostName remoteHostName, string remoteServiceNameOrPort)
        {
            await _socket.ConnectAsync(remoteHostName, remoteServiceNameOrPort);
        }

        public async void SendMessage(string message)
        {
            var stream = _socket.OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteString(message);

            await writer.StoreAsync();
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }

}
