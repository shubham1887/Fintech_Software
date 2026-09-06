using Microsoft.AspNetCore.Http;

namespace FintechApp.Helpers
{
    public static class IPAddressHelper
    {
        public static string GetClientIPAddress(HttpContext context)
        {
            try
            {
                string ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length > 0)
                        return addresses[0].Trim();
                }

                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(ipAddress))
                    return ipAddress;

                ipAddress = context.Connection.RemoteIpAddress?.ToString();
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
