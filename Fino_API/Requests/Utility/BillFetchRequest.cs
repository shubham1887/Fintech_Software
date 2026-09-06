namespace FintechApp.Requests.Utility
{
    public class BillFetchRequest
    {
        public int OperatorID { get; set; }

        /// <summary>Optional — resolved from the operator when omitted.</summary>
        public int ServiceID { get; set; }

        public string ConsumerNumber { get; set; } = string.Empty;

        // Extra biller parameters as defined by the operator's BBPSConfiguration fields
        // (e.g. billing unit, processing cycle) — mapped to [Param1..3] in the endpoint template.
        public string Param1 { get; set; } = string.Empty;
        public string Param2 { get; set; } = string.Empty;
        public string Param3 { get; set; } = string.Empty;
    }
}
