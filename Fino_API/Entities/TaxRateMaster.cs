using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One tax rate, valid for one window of time.
    ///
    /// Rates are never UPDATED. A change closes the current row by setting EffectiveTo and opens
    /// a new one. That is what makes a two-year-old transaction still answer with the rate it was
    /// actually charged: the engine resolves by the transaction's own date, and reports read the
    /// rate stamped on the transaction rather than recomputing it.
    ///
    /// Editing a rate in place instead would silently rewrite history, and every return already
    /// filed would stop reconciling with the books — which is the exact problem this whole
    /// subsystem exists to end.
    /// </summary>
    public class TaxRateMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxRateMasterID { get; set; }

        /// <summary>GST / CESS / TDS / TCS — see StaticModels.TaxType.</summary>
        [Column(TypeName = "varchar(10)")]
        public string TaxType { get; set; } = string.Empty;

        /// <summary>A short handle an admin recognises, e.g. "GST18", "GST5", "TDS194H".</summary>
        [Column(TypeName = "varchar(30)")]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Percent, not a fraction: 18 means 18%.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal Rate { get; set; }

        /// <summary>Compensation cess, where one applies. Normally 0 for these services.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal CessRate { get; set; }

        public DateTime EffectiveFrom { get; set; }

        /// <summary>Null = still current. Set this instead of editing Rate.</summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>The notification or circular this rate comes from, so a number can be defended.</summary>
        [Column(TypeName = "varchar(200)")]
        public string NotificationRef { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
