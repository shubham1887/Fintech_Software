using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using Microsoft.AspNetCore.Http;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IAuditContextService"/>
    public class AuditContextService : IAuditContextService
    {
        private readonly IHttpContextAccessor _accessor;

        public AuditContextService(IHttpContextAccessor accessor) => _accessor = accessor;

        private KeyDataModel? KeyData => _accessor.HttpContext?.Items[AuditContextKeys.KeyData] as KeyDataModel;

        public string IPAddress
        {
            get
            {
                string reported = KeyData?.IPAddress ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(reported)) return Trim(reported);

                var ctx = _accessor.HttpContext;
                if (ctx == null) return string.Empty;

                // Behind IIS / a reverse proxy the socket address is the proxy, so the forwarded
                // header is the only place the real client appears. First hop wins.
                string forwarded = ctx.Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrWhiteSpace(forwarded))
                    return Trim(forwarded.Split(',')[0]);

                return Trim(ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
            }
        }

        public string Platform
        {
            get
            {
                string reported = KeyData?.DeviceType ?? string.Empty;
                return string.IsNullOrWhiteSpace(reported) ? "Web" : Trim(reported, 20);
            }
        }

        private static string Trim(string value, int max = 64)
        {
            value = value.Trim();
            return value.Length <= max ? value : value[..max];
        }
    }

    /// <summary>HttpContext.Items keys shared between the auth filter and the audit context.</summary>
    public static class AuditContextKeys
    {
        public const string KeyData = "KeyData";
    }
}
