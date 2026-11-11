namespace FintechApp.Requests.Phase6
{
    /// <summary>Build a monthly return for one registration.</summary>
    public class GstReturnRequest
    {
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>yyyyMM.</summary>
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>
        /// The B2CL invoice-value limit to apply. 0 uses the current one (Rs 1,00,000). Rebuilding
        /// a period from before Notification 12/2024 must pass 250000, or invoices move between
        /// B2CL and B2CS and the rebuild stops agreeing with the return already filed.
        /// </summary>
        public decimal B2CLThreshold { get; set; }
    }

    /// <summary>Build the annual return for a financial year.</summary>
    public class Gstr9Request
    {
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>The FY start year: 2026 means 2026-27 (April 2026 to March 2027).</summary>
        public int FinancialYearStart { get; set; }
    }

    /// <summary>Keep a generated return, or declare it filed.</summary>
    public class SaveGstReturnRequest
    {
        public string ReturnType { get; set; } = string.Empty;
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>For GSTR-9 the caller passes the FY start year instead of a period key.</summary>
        public int FinancialYearStart { get; set; }

        public decimal B2CLThreshold { get; set; }

        /// <summary>True marks it Filed; false keeps it as a Draft snapshot.</summary>
        public bool MarkFiled { get; set; }

        public string AcknowledgementNo { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    /// <summary>List the snapshots held.</summary>
    public class GstReturnFilingQuery
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
    }

    /// <summary>Produce a downloadable file for a return.</summary>
    public class GstReturnExportRequest
    {
        public string ReturnType { get; set; } = string.Empty;
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public int FinancialYearStart { get; set; }
        public decimal B2CLThreshold { get; set; }

        /// <summary>JSON (the portal's offline-utility shape) or EXCEL (a workbook per table).</summary>
        public string Format { get; set; } = "JSON";
    }
}
