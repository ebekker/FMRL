using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL
{
    public static partial class AppVersion
    {
        public static string GetVersion()
        {
            string version = "(n/a)";
            GetVersionInternal(ref version);
            return version;
        }

        static partial void GetVersionInternal(ref string version);
    }
}
