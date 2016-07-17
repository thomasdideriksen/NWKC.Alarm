using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWKC.Alarm.Common
{
    public class Helpers
    {
        public static double SecondsSinceMidnight(DateTime t)
        {
            return
                t.Hour * 60 * 60 +
                t.Minute * 60 +
                t.Second +
                t.Millisecond / 1000.0;
        }
    }
}
