using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderConfigurationAdminCommission : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderConfigurationAdminCommissionID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public virtual ProviderMaster? ProviderMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminComm { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string AdminCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string AdminCommissionCategory { get; set; } = string.Empty;

        /// <summary>
        /// Whether the charge on THIS band already contains GST — INCLUSIVE / EXCLUSIVE / NONE —
        /// or DEFAULT (the shipped value) to fall through to the commission tax profile.
        ///
        /// A provider's rate card does not necessarily treat every band the same way, and until
        /// this column existed the only place to say so was per provider+service, so a card with
        /// one exempt band could not be expressed at all.
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string GSTType { get; set; } = StaticModels.ChargeGstType.Default;

        /// <summary>
        /// The rate for this band. 0 means "ask the tax engine", which resolves the service's rate
        /// as of the transaction's own date and keeps a future rate change effective-dated in one
        /// place. A number here pins the band to a contract that names a different one.
        /// </summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal GSTRatePercent { get; set; }
    }
}
