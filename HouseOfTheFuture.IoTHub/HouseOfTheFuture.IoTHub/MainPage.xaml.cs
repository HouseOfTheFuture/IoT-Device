using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
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
        private Socket _s;

        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            object token = new object();
            var socketAsyncEventArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.100.53"), 51249)
            };
            socketAsyncEventArgs.SetBuffer(new byte[1024], 0, 1024);
            socketAsyncEventArgs.Completed += (kut, args) =>
            {
                if (args.SocketError == SocketError.Success)
                {
                    var dataDash = args.Buffer;
                    var textReceived = Encoding.ASCII.GetString(dataDash);

                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        kakBox.Text = textReceived;
                    });

                }
                else
                {

                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        kakBox.Text = "Werkt niet :( Maar wel completed  gehit :o";
                    });

                }

            };


            var datagram = new DatagramSocket();
            datagram.

            _s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var result = _s.ReceiveFromAsync(socketAsyncEventArgs);

            var data = socketAsyncEventArgs.Buffer;

            var text = Encoding.ASCII.GetString(data);

            System.Diagnostics.Debug.WriteLine(result);

            kakBox.Text = "Receive : " + result;
        }
    }

    class Jos : SocketAsyncEventArgs
    {
        
    }
}
