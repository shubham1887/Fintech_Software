using FintechApp.Helpers;
using FintechApp.Responses;
using Newtonsoft.Json;
using System.Net;

namespace FintechApp.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
