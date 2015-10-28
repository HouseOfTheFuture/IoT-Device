using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfTheFuture.RF24Library
{
    public static class AddressWidth
    {
        public const int Min = 3;
        public const int Max = 5;


        public static byte Get(byte[] address)
        {
            Check(address);
            return (byte)(address.Length - 2);
        }

        public static void Check(byte[] address)
        {
            Check(address.Length);
        }

        public static void Check(int addressWidth)
        {
            if (addressWidth < Min || addressWidth > Max)
            {
                throw new ArgumentException("Address width needs to be 3-5 bytes");
            }
        }
    }
}
