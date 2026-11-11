using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// An HSN/SAC code and the rate that normally goes with it.
    ///
    /// ServiceMaster already carries an HSNCode string, but a free-typed string per service means
    /// the same service can print one code on an invoice and a different one in the GSTR-1 HSN
    /// summary, and nobody finds out until the return is filed. The codes live here once; a
    /// service (and a tax rule) points at a row.
    ///
    /// The codes that actually appear in this business, taken from the suppliers' own invoices:
    ///   998413  telecom / recharge / e-topup        (Ezytm, Erech Trade API)
    ///   997112  financial services, DMT, payout     (JaiJui "Payout charge")
    ///   997119  other financial services, AEPS
    ///   997159  services auxiliary to finance       (JaiJui "VERIFICATION")
    ///   998599  other support services, subscription
    /// </summary>
    public class HSNSACMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HSNSACMasterID { get; set; }

        /// <summary>The 6-digit SAC (or HSN) as it must print on the invoice.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "varchar(300)")]
        public string Description { get; set; } = string.Empty;

        /// <summary>The rate this code normally attracts. A rule may still override it.</summary>
        public int? DefaultTaxRateMasterID { get; set; }

        [ForeignKey(nameof(DefaultTaxRateMasterID))]
        public virtual TaxRateMaster? DefaultTaxRate { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
