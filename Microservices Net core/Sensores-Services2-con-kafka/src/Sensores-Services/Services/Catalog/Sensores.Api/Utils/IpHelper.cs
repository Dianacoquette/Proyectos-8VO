using System.Net;

namespace Sensores.Api.Utils
{
    public static class IpHelper
    {
        public static long ConvertIpToNumber(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return 0;

            if (!IPAddress.TryParse(ip, out var ipAddress))
                return 0;

            var bytes = ipAddress.MapToIPv4().GetAddressBytes();

            return ((long)bytes[0] << 24)
                 + ((long)bytes[1] << 16)
                 + ((long)bytes[2] << 8)
                 + bytes[3];
        }
    }
}