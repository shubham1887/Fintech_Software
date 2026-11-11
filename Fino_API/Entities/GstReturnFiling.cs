using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One generation of a GST return, kept because a filed return stops being a derivation and
    /// becomes a fact.
    ///
    /// GSTR-9 has to report the year BOTH as the books say and as the returns actually filed said.
    /// Recomputing the second from today's rules would answer the first question twice and hide
    /// exactly the difference the annual return exists to surface — so what went to the portal is
    /// snapshotted here instead.
    /// </summary>
    public class GstReturnFiling : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GstReturnFilingID { get; set; }

        /// <summary>GSTR1 / GSTR3B / GSTR9.</summary>
        [Column(TypeName = "varchar(10)")]
        public string ReturnType { get; set; } = string.Empty;

        /// <summary>yyyyMM for the monthly returns; the FY start year followed by 0000 for GSTR-9
        /// (20260000 is FY 2026-27), so one column sorts and groups every return alike.</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string CompanyGSTIN { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTaxableValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCess { get; set; }

        public int DocumentCount { get; set; }

        /// <summary>The return in the portal's own shape, as it was generated.</summary>
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>Draft / Filed / Superseded.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = StaticModels.GstFilingStatus.Draft;

        public DateTime? FiledOn { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string AcknowledgementNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(300)")]
        public string Note { get; set; } = string.Empty;

        public int CreatedByUserID { get; set; }
    }
}
