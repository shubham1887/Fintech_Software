using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One import of a GSTR-2B statement, for one registration and one return period.
    ///
    /// 2B is the government's view of our purchases — what our suppliers declared about us. It is
    /// static once generated (unlike 2A), so an import is a snapshot: re-importing the same period
    /// supersedes the previous snapshot rather than merging into it, because a later 2B may drop a
    /// line a supplier amended. The lines live in <see cref="Gstr2bLine"/>.
    /// </summary>
    public class Gstr2bImport : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Gstr2bImportID { get; set; }

        /// <summary>Our registration this 2B belongs to.</summary>
        [Column(TypeName = "varchar(15)")]
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>The return period, as yyyyMM — the same key TaxInvoiceHeader.PeriodKey uses, so
        /// a period can be joined on both sides without reformatting.</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>When the portal generated this 2B (its gendt), not when we imported it.</summary>
        public DateTime? GeneratedDate { get; set; }

        /// <summary>JSON (portal download) or Manual.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Source { get; set; } = string.Empty;

        public int LineCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTaxable { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTax { get; set; }

        /// <summary>Active / Superseded. A re-import of the same period marks the old one Superseded
        /// so the recon only ever sees one snapshot, but the old one is still there to look back at.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        public int ImportedByUserID { get; set; }
    }
}
