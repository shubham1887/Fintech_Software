using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One numbering series for one kind of GST document.
    ///
    /// GST requires an invoice number that is unique, consecutive and reset per financial year,
    /// at most 16 characters, using only letters, digits, "/" and "-". The existing
    /// InvoiceNumberHistory numbers per user per service per MONTH, which produces a separate
    /// series for every combination — that cannot satisfy "consecutive", and it makes GSTR-1's
    /// "Documents Issued" table (series, from, to, cancelled) impossible to fill. This replaces it.
    ///
    /// CurrentNumber is the live counter and is allocated inside the database, never from a
    /// counter held in application memory: two web workers, or one worker and a month-end batch,
    /// would otherwise hand out the same number twice, and a duplicate invoice number is the most
    /// expensive mistake available here.
    /// </summary>
    public class TaxDocumentSeries : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxDocumentSeriesID { get; set; }

        /// <summary>Stable handle used in code and config, e.g. "SALE-B2B".</summary>
        [Column(TypeName = "varchar(40)")]
        public string SeriesCode { get; set; } = string.Empty;

        /// <summary>TaxInvoice / CreditNote / DebitNote / BillOfSupply / SelfInvoice.</summary>
        [Column(TypeName = "varchar(20)")]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>Which registration issues under this series. One series per GSTIN: a number
        /// is only unique within the registration that raised it.</summary>
        [Column(TypeName = "varchar(15)")]
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>Printed before the number, e.g. "SS/". Keep it short — prefix plus the padded
        /// number plus the year must stay inside 16 characters.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Prefix { get; set; } = string.Empty;

        /// <summary>"2526" for FY 2025-26. Part of the number, and what the yearly reset keys on.</summary>
        [Column(TypeName = "varchar(8)")]
        public string FinancialYear { get; set; } = string.Empty;

        /// <summary>The last number ISSUED. The next document takes this + 1, allocated atomically.</summary>
        public int CurrentNumber { get; set; }

        /// <summary>How many digits the number is padded to, e.g. 5 -> "00042".</summary>
        public int NumberLength { get; set; } = 5;

        /// <summary>FinancialYear or Never. GST expects a yearly reset.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ResetPolicy { get; set; } = "FinancialYear";

        public bool IsActive { get; set; } = true;
    }
}
