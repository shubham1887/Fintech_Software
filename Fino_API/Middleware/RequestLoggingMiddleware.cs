using FintechApp.Helpers;

namespace FintechApp.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logger _logger;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = new Logger();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var ipAddress = IPAddressHelper.GetClientIPAddress(context);

                var logMessage = $"Request: {request.Method} {request.Path} from IP: {ipAddress}";
                _logger.Write(logMessage);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                throw;
            }
        }
    }
}
