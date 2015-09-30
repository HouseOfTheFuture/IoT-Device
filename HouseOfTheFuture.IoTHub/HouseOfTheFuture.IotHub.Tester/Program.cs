using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfTheFuture.IotHub.Tester
{
    class Program
    {

        static void Main(string[] args)
        {
            var str4 = "Smart";
            var sendBytes4 = new byte[1024];

            //var udpBroadcast = new System.Net.Sockets.UdpClient(11001); // local binding
            //udpBroadcast.Connect(IPAddress.Broadcast, 11000);
            //udpBroadcast.Send(sendBytes4, sendBytes4.Length);
            //udpBroadcast.Close();

            Console.ReadLine();

            var asyncshit = new SocketAsyncEventArgs();
            asyncshit.RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 51249);
            asyncshit.Completed += (sender, eventArgs) =>
            {
                Console.WriteLine("PALJAS");
            };

            asyncshit.SetBuffer(sendBytes4, 0, sendBytes4.GetLength(0));

            var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SendToAsync(asyncshit);

            Console.ReadLine();
        }
    }
}
