using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

namespace PetCareSystem.API.Helpers
{
    public static class VnpayHelper
    {
        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
                }
            }
            catch
            {
                ipAddress = "127.0.0.1";
            }

            return ipAddress;
        }
    }
}
