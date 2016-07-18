using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace NWKC.Alarm.Common
{
    public static class Helpers
    {
        public static double ToUnixTimeInSeconds(this DateTime t)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var elapsed = t.Subtract(unixEpoch);
            return elapsed.TotalSeconds;
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
