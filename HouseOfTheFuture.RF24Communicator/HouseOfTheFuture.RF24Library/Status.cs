using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfTheFuture.RF24Library
{
    public class Status
    {
        private byte _reg;

        public bool DataReady { get { return (_reg & (1 << Bits.RX_DR)) > 0; } }
        public bool DataSent { get { return (_reg & (1 << Bits.TX_DS)) > 0; } }
        public bool ResendLimitReached { get { return (_reg & (1 << Bits.MAX_RT)) > 0; } }
        public bool TxFull { get { return (_reg & (1 << Bits.TX_FULL)) > 0; } }
        public byte DataPipe { get { return (byte)((_reg >> 1) & 7); } }
        public bool DataPipeNotUsed { get { return DataPipe == 6; } }
        public bool RxEmpty { get { return DataPipe == 7; } }

        public Status(byte reg)
        {
            _reg = reg;
        }

        public void Update(byte reg)
        {
            _reg = reg;
        }

        public override string ToString()
        {
            return "DataReady: " + DataReady +
                   ", DateSent: " + DataSent +
                   ", ResendLimitReached: " + ResendLimitReached +
                   ", TxFull: " + TxFull +
                   ", RxEmpty: " + RxEmpty +
                   ", DataPipe: " + DataPipe +
                   ", DataPipeNotUsed: " + DataPipeNotUsed;
        }
    }
}
