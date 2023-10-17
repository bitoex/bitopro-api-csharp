using log4net;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Bitopro
{
    public class Utils
    {
        public static ILog Logger = LogManager.GetLogger("BitoproApp");
        /// <summary>
        /// Gets a HMACSHA256 signature based on the API Secret.
        /// </summary>
        /// <param name="apiSecret">Api secret used to generate the signature.</param>
        /// <param name="message">Message to encode.</param>
        /// <returns></returns>
        public static string GenerateSignature(string apiSecret, string message)
        {
            var key = Encoding.UTF8.GetBytes(apiSecret);
            string stringHash;
            using (var hmac = new HMACSHA384(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                stringHash = BitConverter.ToString(hash).Replace("-", "");
            }

            return stringHash;
        }

        /// <summary>
        /// Gets a timestamp in milliseconds.
        /// </summary>
        /// <returns>Timestamp in milliseconds.</returns>
        public static long GenerateTimeStamp(DateTime baseDateTime)
        {
            if (baseDateTime == DateTime.MinValue)
                return 0;

            var dtOffset = new DateTimeOffset(baseDateTime);
            return dtOffset.ToUnixTimeMilliseconds();
        }
    }
}
