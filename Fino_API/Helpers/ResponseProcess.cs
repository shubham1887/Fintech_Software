using FintechApp.Responses;
using FintechApp.StaticModels;

namespace FintechApp.Helpers
{
    public static class ResponseProcess
    {
        public static APIResponse GetSuccessResponse(string data = "", string message = Messages.Common.Message.Success)
        {
            return new APIResponse
            {
                StatusCode = 200,
                Status = StatusType.Success,
                Message = message,
                Data = data
            };
        }

        public static APIResponse GetFailResponse(string message = Messages.Common.Message.Failed)
        {
            return new APIResponse
            {
                StatusCode = 400,
                Status = StatusType.Failed,
                Message = message,
                Data = string.Empty
            };
        }

        public static APIResponse GetUnauthorizedResponse(string message = Messages.Common.Message.Unauthorized)
        {
            return new APIResponse
            {
                StatusCode = 401,
                Status = StatusType.Unauthorized,
                Message = message,
                Data = string.Empty
            };
        }

        public static APIResponse GetErrorResponse(string message = Messages.Common.Message.ExceptionOccured)
        {
            return new APIResponse
            {
                StatusCode = 500,
                Status = StatusType.Error,
                Message = message,
                Data = string.Empty
            };
        }
    }
}
