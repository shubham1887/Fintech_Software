namespace FintechApp.Requests.Plan
{
    /// <summary>
    /// What the app's own clients send to the plan / R-Offer / DTH endpoints. It extends the
    /// provider library's request with the fields only this application knows about.
    ///
    /// Clients should send <see cref="OperatorID"/> (their OperatorMaster row) and leave
    /// OperatorCode empty: the plan provider uses its own operator codes, so the server resolves
    /// OperatorID → provider OpCode. OperatorCode is still honoured when it already carries a code
    /// the provider publishes, which keeps server-to-server callers working unchanged.
    /// </summary>
    public class PlanFetchAppRequest : Fino.PlanApi.Requests.PlanFetchRequest
    {
        /// <summary>The caller's own OperatorMaster id, resolved server-side to the provider code.</summary>
        public int OperatorID { get; set; }

        /// <summary>Optional filter for the operator-code catalog lookup (e.g. "DTH", "PREPAID").</summary>
        public string Type { get; set; } = string.Empty;
    }
}
