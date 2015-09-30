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
            var udp = new UdpClient();
            var groupEP = new IPEndPoint(IPAddress.Broadcast, GroupPort);

            var str4 = "Smart";

            var sendBytes4 = Encoding.ASCII.GetBytes(str4);

            udp.Send(sendBytes4, sendBytes4.Length, groupEP);
            byte[] receiveBytes = udp.Receive(ref groupEP);
        }
    }
}
