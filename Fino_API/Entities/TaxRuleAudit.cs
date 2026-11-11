using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// Every change to a tax rule, rate, SAC code or TDS section.
    ///
    /// A tax rule decides money. "Who changed the recharge basis from gross to commission, and
    /// when" is the first question asked in an audit and the first question asked when a month's
    /// numbers stop making sense, and neither has an answer unless it was written down as it
    /// happened. Rules are effective-dated so nothing is overwritten, but the dating says what
    /// the rule became — not who decided, or why.
    /// </summary>
    public class TaxRuleAudit : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxRuleAuditID { get; set; }

        /// <summary>Which master changed: TaxRuleMaster / TaxRateMaster / HSNSACMaster /
        /// TDSSectionMaster / PartyTaxProfile / PartyServiceRelationship.</summary>
        [Column(TypeName = "varchar(40)")]
        public string EntityName { get; set; } = string.Empty;

        public int EntityID { get; set; }

        /// <summary>CREATE / UPDATE / DEACTIVATE.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Action { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string OldValueJson { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string NewValueJson { get; set; } = string.Empty;

        /// <summary>Why. A rate change with no reason is a rate change nobody can defend.</summary>
        [Column(TypeName = "varchar(500)")]
        public string Reason { get; set; } = string.Empty;

        public int ChangedByUserID { get; set; }

        public DateTime ChangedOn { get; set; }
    }
}
