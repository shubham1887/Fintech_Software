using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// The tax identity of one party, kept apart from UserMaster.
    ///
    /// UserMaster already holds GSTNo, PANNo and BusinessGSTStateCode, but those are typed in at
    /// onboarding and nothing records whether anyone ever CHECKED them. A GSTIN that is merely
    /// present is not a GSTIN that exists, and the difference decides whether an invoice is a
    /// tax invoice or a B2C bill. The verification state, the 206AB flag and any lower-deduction
    /// certificate live here, where they can be re-verified on a schedule without touching the
    /// user record every report selects in bulk.
    /// </summary>
    public class PartyTaxProfile : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartyTaxProfileID { get; set; }

        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual UserMaster? UserMaster { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string GSTIN { get; set; } = string.Empty;

        /// <summary>REGISTERED / UNREGISTERED / COMPOSITION / SEZ / OVERSEAS.</summary>
        [Column(TypeName = "varchar(20)")]
        public string RegistrationStatus { get; set; } = string.Empty;

        public DateTime? GSTINVerifiedOn { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string PAN { get; set; } = string.Empty;

        public DateTime? PANVerifiedOn { get; set; }

        /// <summary>Section 206AB: a non-filer, so a higher TDS rate. Verified on the compliance
        /// check portal, not guessed.</summary>
        public bool Is206ABFlagged { get; set; }

        public DateTime? ComplianceCheckedOn { get; set; }

        // ── Section 197 lower-deduction certificate ─────────────────────────────────────
        // A certificate is capped: it is valid until a date AND only up to an amount. Both have
        // to be tracked, because deducting at the certificate rate past either limit is a
        // short-deduction the department recovers with interest.

        [Column(TypeName = "varchar(30)")]
        public string LowerDeductionCertNo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal LDCRate { get; set; }

        public DateTime? LDCValidFrom { get; set; }

        public DateTime? LDCValidTill { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LDCAmountCap { get; set; }

        /// <summary>The state code the invoice must use. Cross-checked against the first two
        /// digits of the GSTIN, because a mismatch sends the tax to the wrong head.</summary>
        [Column(TypeName = "varchar(2)")]
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        /// <summary>Both legs of a reciprocal wallet swap must be invoiced for this party.
        /// Under GST Section 7(1)(a) barter is a supply and each side is taxable, so a partner
        /// whose incoming credit was recorded while the outgoing one was not is turnover that
        /// left the books entirely.</summary>
        public bool IsExchangePartner { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string ExchangeAgreementRef { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;
    }
}
