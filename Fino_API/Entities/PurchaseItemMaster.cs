using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One thing a supplier bills us for — "E-Topup", "Domestic Money Transfer", "Bill Payment".
    ///
    /// Picking it on a purchase line fills the SAC, the GST rate and the usual rate, so a bill is
    /// entered as "what, how much" instead of every tax head by hand. The list belongs to the
    /// install: it is edited from the Purchase Invoice screen, because a supplier who starts
    /// billing something new should not need a release.
    /// </summary>
    public class PurchaseItemMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseItemMasterID { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string SACCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal GSTRatePercent { get; set; } = 18m;

        /// <summary>The supplier's usual rate for this item; 0 = typed on the bill each time.</summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal DefaultRate { get; set; }

        /// <summary><c>PERCENT</c> (amount = quantity × rate ÷ 100) or <c>UNIT</c> (quantity × rate).</summary>
        [Column(TypeName = "varchar(10)")]
        public string RateType { get; set; } = PurchaseRateType.Percent;

        public bool ITCEligible { get; set; } = true;
        public int UISequence { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public static class PurchaseRateType
    {
        public const string Percent = "PERCENT";
        public const string Unit = "UNIT";

        public static string Normalise(string? value) =>
            string.Equals(value?.Trim(), Unit, StringComparison.OrdinalIgnoreCase) ? Unit : Percent;
    }
}
