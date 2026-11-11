using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// Whether one party acts FOR us or trades WITH us, on one service, for a window of time.
    ///
    /// This is the single most valuable field on the TDS side. The Supreme Court held in Bharti
    /// Cellular (2024) that the discount a telecom company gives a prepaid distributor is not
    /// "commission" under Section 194H, because the relationship is principal-to-principal rather
    /// than principal-agent — the test being Section 182 of the Contract Act. So:
    ///
    ///   recharge margin, P2P    -> a discount  -> no 194H
    ///   DMT / AEPS commission   -> agency      -> 194H at 2%
    ///
    /// The same retailer can be both at once, on different services, which is why this is keyed
    /// per party AND per service rather than sitting on the user or the service alone.
    ///
    /// AgreementRef is not decoration. Pure-agent and principal-to-principal treatment both rest
    /// on what the contract actually says; a row with no agreement behind it is an assertion, and
    /// an assertion is what gets disallowed.
    /// </summary>
    public class PartyServiceRelationship : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartyServiceRelationshipID { get; set; }

        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual UserMaster? UserMaster { get; set; }

        /// <summary>0 = every service, for a party whose whole relationship is one kind.</summary>
        public int ServiceID { get; set; }

        /// <summary>P2P or AGENT — see StaticModels.PartyRelationship.</summary>
        [Column(TypeName = "varchar(10)")]
        public string RelationshipType { get; set; } = string.Empty;

        /// <summary>The agreement this rests on. Required in practice, enforced by review.</summary>
        [Column(TypeName = "varchar(200)")]
        public string AgreementRef { get; set; } = string.Empty;

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
