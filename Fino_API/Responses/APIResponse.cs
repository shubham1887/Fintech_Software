namespace FintechApp.Responses
{
    public class APIResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        public APIResponse()
        {
        }

        public APIResponse(int statusCode, string status, string message, string data = "")
        {
            StatusCode = statusCode;
            Status = status;
            Message = message;
            Data = data;
        }
    }
}
