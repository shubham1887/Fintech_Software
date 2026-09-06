namespace Fino.PlanApi.Requests
{
    // Inputs for the plan-data calls. Operator/circle/number are supplied per request;
    // provider credentials and base URL are resolved dynamically from configuration.
    public class PlanFetchRequest
    {
        public string OperatorCode { get; set; } = string.Empty;
        public string Circle { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string OpCode { get; set; } = string.Empty;
        public string DthNumber { get; set; } = string.Empty;
    }
}
