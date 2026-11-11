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

        // --- Electricity bill fetch -----------------------------------------------------------
        // The provider names the three extra inputs Optional1..3 because their meaning changes per
        // discom. The named properties below are what a caller should actually set; Optional1..3
        // stay as an escape hatch for a discom needing something not yet named here.

        /// <summary>Consumer / bill / K-number printed on the bill (the provider's bill_number).</summary>
        public string BillNumber { get; set; } = string.Empty;

        /// <summary>Optional1 — district/discom code from ElectricityDistrictCode ("Agra-DVVNL").</summary>
        public string DistrictCode { get; set; } = string.Empty;

        /// <summary>Optional2 — unit / processing-unit number printed on the bill (e.g. "0850" for MSEDCL).</summary>
        public string UnitNumber { get; set; } = string.Empty;

        /// <summary>Raw Optional1, used only when <see cref="DistrictCode"/> is empty.</summary>
        public string Optional1 { get; set; } = string.Empty;

        /// <summary>Raw Optional2, used only when <see cref="UnitNumber"/> is empty.</summary>
        public string Optional2 { get; set; } = string.Empty;

        /// <summary>Raw Optional3 — forwarded as-is; no discom in use needs it today.</summary>
        public string Optional3 { get; set; } = string.Empty;

        /// <summary>Provider circle id for the district-code lookup.</summary>
        public string CircleId { get; set; } = string.Empty;

        /// <summary>Optional1 to send: the named district code wins, else whatever was put in Optional1.</summary>
        public string EffectiveOptional1()
            => DistrictCode.Trim().Length > 0 ? DistrictCode.Trim() : (Optional1 ?? string.Empty).Trim();

        /// <summary>Optional2 to send: the named unit number wins, else whatever was put in Optional2.</summary>
        public string EffectiveOptional2()
            => UnitNumber.Trim().Length > 0 ? UnitNumber.Trim() : (Optional2 ?? string.Empty).Trim();
    }
}
