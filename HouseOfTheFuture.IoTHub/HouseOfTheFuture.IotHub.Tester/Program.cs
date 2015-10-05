using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Test();
            Console.Read();
        }


        public static async Task Test()
        {
            using (var udp = new UdpClient())
            {
                var listenEndpoint = new IPEndPoint(IPAddress.Any, 5321);

                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.ExclusiveAddressUse = false;
                udp.Client.Bind(listenEndpoint);
                while (true)
                {
                    var response = await udp.ReceiveAsync();
                    Console.WriteLine(response.RemoteEndPoint.Address.ToString());
                }
            }
        }
    }
}
