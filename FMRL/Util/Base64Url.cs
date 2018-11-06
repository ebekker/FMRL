﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL.Util
{
    // Thanks to Brock Allen at:
    //    https://brockallen.com/2014/10/17/base64url-encoding/
    public static class Base64Url
    {
        public static string ToBase64UrlString(this byte[] arg)
        {
            string s = Convert.ToBase64String(arg); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        public static byte[] FromBase64UrlString(this string arg)
        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(s); // Standard base64 decoder
        }
    }
}
