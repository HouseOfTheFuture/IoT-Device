using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfTheFuture.RF24Library
{
    public enum AddressSlot
        {
            Zero = Registers.RX_ADDR_P0,
            One = Registers.RX_ADDR_P1,
            Two = Registers.RX_ADDR_P2,
            Three = Registers.RX_ADDR_P3,
            Four = Registers.RX_ADDR_P4,
            Five = Registers.RX_ADDR_P5,
        }
}
