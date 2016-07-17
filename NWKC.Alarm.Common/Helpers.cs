using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

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
        
        public static Stream GetEmbeddedResource(string name, Assembly assembly)
        {
            var names = assembly.GetManifestResourceNames();
            foreach (var candidate in names)
            {
                if (candidate.ToLower().EndsWith(name.ToLower()))
                {
                    return assembly.GetManifestResourceStream(candidate);
                }
            }
            return null;
        }
    }
}
