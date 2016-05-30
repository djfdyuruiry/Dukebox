using System;
using System.Text;

namespace Dukebox.Library.Helper
{
    public static class Base64Decoder
    {
        public static string Base64Decode(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                throw new ArgumentOutOfRangeException("Base64 encoded data string was null or empty");
            }

            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string DoubleBase64Decode(string base64EncodedData)
        {
            var firstPass = Base64Decode(base64EncodedData);
            return Base64Decode(firstPass);
        }
    }
}
