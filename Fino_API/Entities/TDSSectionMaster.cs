using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One TDS/TCS section, with its rate and threshold for a window of time.
    ///
    /// The section is a row, not a string in code, for a concrete reason: from 1 April 2026 the
    /// Income-tax Act, 2025 renumbers the sections (194H is cited as 393(1) Table Sl. No. 1(ii)),
    /// while the rate and threshold stay the same. A renumbering should be an admin edit, not a
    /// release.
    ///
    /// ThresholdAnnual is deliberately annual. Section 194H's Rs 20,000 is tested on the YEAR's
    /// aggregate per party, not per payment, so the engine needs a running year-to-date total —
    /// checking each payment on its own under-deducts for every party that crosses the line in
    /// small amounts.
    /// </summary>
    public class TDSSectionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TDSSectionMasterID { get; set; }

        /// <summary>As people say it: "194H", "194C", "194J", "194O", "194Q", "206C(1H)".</summary>
        [Column(TypeName = "varchar(20)")]
        public string SectionCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Normal rate for a resident with a valid PAN. Percent: 2 means 2%.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal Rate { get; set; }

        /// <summary>No deduction until the year's aggregate for one party passes this.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThresholdAnnual { get; set; }

        /// <summary>Section 206AA — no PAN, so 20%.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal NoPANRate { get; set; }

        /// <summary>Section 206AB — a non-filer pays a higher rate. 0 when it does not apply.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal NonFilerRate { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
