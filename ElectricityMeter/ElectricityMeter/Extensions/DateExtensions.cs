using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricityMeter
{
    public static class DateExtensions
    {
        public static ulong TotalMilliseconds(this DateTime date)
        {
            return Convert.ToUInt64(date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        }
    }
}
