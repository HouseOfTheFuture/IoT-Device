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
        public static int GroupPort { get; private set; }

        static void Main(string[] args)
        {
            GroupPort = 5321;

            var udp = new UdpClient();
            var groupEP = new IPEndPoint(IPAddress.Any, GroupPort);

            var str4 = "Smart";

            //var sendBytes4 = Encoding.ASCII.GetBytes(str4);

            //udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.ExclusiveAddressUse = false;
            udp.Client.Bind(groupEP);
            udp.BeginReceive(new AsyncCallback(re), null);

            Console.Read();
        }

        private static void re(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }
    }
}
