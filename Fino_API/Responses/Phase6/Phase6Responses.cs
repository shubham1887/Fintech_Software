namespace FintechApp.Responses.Phase6
{
    // ── Shared ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One rate slab's figures. Every GSTR-1 table breaks down to this, because the portal reports
    /// per rate rather than per document — two lines of the same invoice at different rates are
    /// two rows in the return.
    /// </summary>
    public class RateSlab
    {
        /// <summary>The combined rate: IGST, or CGST + SGST. 18, not 9 + 9.</summary>
        public decimal Rate { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Cess { get; set; }
    }

    // ── GSTR-1 ────────────────────────────────────────────────────────────────────────────

    /// <summary>One document inside a B2B / B2CL / CDNR table.</summary>
    public class Gstr1Document
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public decimal InvoiceValue { get; set; }
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
        public string SupplyNature { get; set; } = string.Empty;
        public bool ReverseCharge { get; set; }

        /// <summary>For a credit or debit note: the document it adjusts.</summary>
        public string OriginalDocumentNo { get; set; } = string.Empty;
        public DateTime? OriginalDocumentDate { get; set; }
        public string NoteReason { get; set; } = string.Empty;

        public List<RateSlab> Slabs { get; set; } = new();
    }

    /// <summary>A registered counterparty's documents, which is how B2B and CDNR are grouped.</summary>
    public class Gstr1Party
    {
        public string GSTIN { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal TotalTax { get; set; }
        public List<Gstr1Document> Documents { get; set; } = new();
    }

    /// <summary>A B2CS row: no document numbers, because the table is a consolidation.</summary>
    public class Gstr1B2csRow
    {
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
        public string PlaceOfSupplyName { get; set; } = string.Empty;
        /// <summary>INTER or INTRA — the portal's own split for this table.</summary>
        public string SupplyType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Cess { get; set; }
        /// <summary>How many documents were folded into this row. Not filed — it is here so a
        /// figure on the screen can be traced back to something.</summary>
        public int DocumentCount { get; set; }
    }

    /// <summary>Table 12: the HSN/SAC summary.</summary>
    public class Gstr1HsnRow
    {
        public string SACCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Cess { get; set; }
    }

    /// <summary>Table 13: a series, the numbers it actually used, and how many were cancelled.</summary>
    public class Gstr1DocSeriesRow
    {
        public string SeriesCode { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
        public string ToNumber { get; set; } = string.Empty;
        public int TotalIssued { get; set; }
        public int Cancelled { get; set; }
        public int NetIssued { get; set; }
    }

    /// <summary>Table 8: nil-rated, exempt and non-GST outward supplies.</summary>
    public class Gstr1ExemptRow
    {
        public string Category { get; set; } = string.Empty;
        public decimal NilRated { get; set; }
        public decimal Exempted { get; set; }
        public decimal NonGST { get; set; }
    }

    public class Gstr1Result
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        /// <summary>The B2CL limit this build used. Reported because it has changed over time and
        /// a rebuild on the wrong one silently moves invoices between two tables.</summary>
        public decimal B2CLThreshold { get; set; }

        public List<Gstr1Party> B2B { get; set; } = new();
        public List<Gstr1Party> B2CL { get; set; } = new();
        public List<Gstr1B2csRow> B2CS { get; set; } = new();
        public List<Gstr1Party> CDNR { get; set; } = new();
        public List<Gstr1Party> CDNUR { get; set; } = new();
        public List<Gstr1ExemptRow> Exempt { get; set; } = new();
        public List<Gstr1HsnRow> HSN { get; set; } = new();
        public List<Gstr1DocSeriesRow> Documents { get; set; } = new();

        /// <summary>Totals across every table, for the one line an accountant checks first.</summary>
        public decimal TotalTaxableValue { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalCess { get; set; }
        public int TotalDocuments { get; set; }

        /// <summary>Documents that could not be placed in a table, and why. Never silently
        /// dropped: a supply missing from the return is the error nobody sees.</summary>
        public List<string> Warnings { get; set; } = new();
    }

    // ── GSTR-3B ───────────────────────────────────────────────────────────────────────────

    /// <summary>One line of 3B table 3.1 — the outward-supply split the portal asks for.</summary>
    public class Gstr3bOutwardRow
    {
        /// <summary>a / b / c / d / e, as the portal labels them.</summary>
        public string Code { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Cess { get; set; }
    }

    /// <summary>Table 3.2: of the 3.1(a) supplies, the part made to unregistered persons in
    /// another state, broken down by place of supply.</summary>
    public class Gstr3bInterStateRow
    {
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
        public string PlaceOfSupplyName { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public decimal IGST { get; set; }
    }

    /// <summary>Table 4: the input tax credit.</summary>
    public class Gstr3bItcRow
    {
        public string Code { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Cess { get; set; }
    }

    public class Gstr3bResult
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;

        public List<Gstr3bOutwardRow> Outward { get; set; } = new();
        public List<Gstr3bInterStateRow> InterStateSupplies { get; set; } = new();
        public List<Gstr3bItcRow> Itc { get; set; } = new();

        /// <summary>Output tax, net credit, and what is left to pay. The three figures the return
        /// is actually about.</summary>
        public decimal OutputIGST { get; set; }
        public decimal OutputCGST { get; set; }
        public decimal OutputSGST { get; set; }
        public decimal OutputCess { get; set; }

        public decimal NetItcIGST { get; set; }
        public decimal NetItcCGST { get; set; }
        public decimal NetItcSGST { get; set; }
        public decimal NetItcCess { get; set; }

        public decimal PayableIGST { get; set; }
        public decimal PayableCGST { get; set; }
        public decimal PayableSGST { get; set; }
        public decimal PayableCess { get; set; }
        public decimal TotalPayable { get; set; }

        /// <summary>Credit left unused by earlier months (as this system computes it) and brought into this one.</summary>
        public decimal OpeningCreditIGST { get; set; }
        public decimal OpeningCreditCGST { get; set; }
        public decimal OpeningCreditSGST { get; set; }
        public decimal OpeningCreditCess { get; set; }

        /// <summary>Credit still unused after this month's tax — it carries to the next month.</summary>
        public decimal ClosingCreditIGST { get; set; }
        public decimal ClosingCreditCGST { get; set; }
        public decimal ClosingCreditSGST { get; set; }
        public decimal ClosingCreditCess { get; set; }

        /// <summary>
        /// 3B is a summary of the same supplies GSTR-1 reports one by one, so the two must agree.
        /// The difference is computed here rather than left for someone to notice, because a 1-vs-3B
        /// mismatch is the single most common notice a taxpayer receives.
        /// </summary>
        public decimal Gstr1TaxableValue { get; set; }
        public decimal Gstr1TotalTax { get; set; }
        public decimal VarianceTaxableValue { get; set; }
        public decimal VarianceTax { get; set; }
        public bool AgreesWithGstr1 { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    // ── GSTR-9 ────────────────────────────────────────────────────────────────────────────

    /// <summary>One month of the year, with books beside what was filed.</summary>
    public class Gstr9MonthRow
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;

        public decimal BooksTaxableValue { get; set; }
        public decimal BooksTax { get; set; }

        /// <summary>From the saved GSTR-1 snapshot, when there is one.</summary>
        public decimal FiledTaxableValue { get; set; }
        public decimal FiledTax { get; set; }
        public bool WasFiled { get; set; }

        public decimal VarianceTaxableValue { get; set; }
        public decimal VarianceTax { get; set; }
    }

    public class Gstr9Result
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        /// <summary>The financial year's start year: 2026 means 2026-27.</summary>
        public int FinancialYearStart { get; set; }
        public string FinancialYearLabel { get; set; } = string.Empty;

        public List<Gstr9MonthRow> Months { get; set; } = new();

        /// <summary>Part II — outward supplies for the year, by rate.</summary>
        public List<RateSlab> OutwardByRate { get; set; } = new();

        /// <summary>Part III — the credit availed for the year.</summary>
        public decimal ItcAvailedIGST { get; set; }
        public decimal ItcAvailedCGST { get; set; }
        public decimal ItcAvailedSGST { get; set; }
        public decimal ItcAvailedCess { get; set; }
        public decimal ItcReversed { get; set; }

        public decimal TotalTurnover { get; set; }
        public decimal TotalTaxPayable { get; set; }

        /// <summary>Books against returns, for the whole year. The number Part II exists for.</summary>
        public decimal TotalFiledTaxableValue { get; set; }
        public decimal TotalVarianceTaxableValue { get; set; }
        public int MonthsFiled { get; set; }
        public int MonthsMissing { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    // ── Filings ───────────────────────────────────────────────────────────────────────────

    public class GstReturnFilingRow
    {
        public int GstReturnFilingID { get; set; }
        public string ReturnType { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public decimal TotalTaxableValue { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalCess { get; set; }
        public int DocumentCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? FiledOn { get; set; }
        public string AcknowledgementNo { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime GeneratedOn { get; set; }
    }

    /// <summary>A generated file, carried back through the encrypted envelope as base64 — the same
    /// way the statement and 2B imports carry one in.</summary>
    public class GstReturnExport
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
        public int ByteCount { get; set; }
    }
}
