namespace Fino.PlanApi.Responses
{
    // Result of a plan-data call. Data carries the raw provider JSON on success.
    public class PlanApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        public static PlanApiResponse Ok(string data) => new() { Success = true, Data = data };
        public static PlanApiResponse Fail(string message) => new() { Success = false, Message = message };
    }
}
