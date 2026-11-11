using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ServiceMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")] 
        public string Name { get; set; }  = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ServiceCategory { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string ServiceImagePath { get; set; } = string.Empty;

        [DefaultValue(0)]
        public int UISequence { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string HSNCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string HSNCodeForSelfInvoice { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinBalance { get; set; }

        public bool IsDelete { get; set; }

        public bool IsActive { get; set; }

        [DefaultValue(true)]
        public bool IsDisplay { get; set; }

        [DefaultValue(true)]
        public bool IsDefault { get; set; }

        [DefaultValue(true)]
        public bool IsOutside { get; set; }

        [DefaultValue(false)]
        public bool? IsBBPS { get; set; }

        [DefaultValue(true)]
        public bool IsSubscriptionAllow { get; set; }

        // true  = charge is GST-inclusive (In. GST) — GST is already contained in Charge.
        // false = charge is GST-exclusive (Ex. GST) — GST is added on top of Charge.
        [DefaultValue(false)]
        public bool IsGSTInclusive { get; set; }

        /// <summary>
        /// KYC exemption. When the platform enforces KYC (Key Setting "IsKYCEnforced"), a user
        /// whose KYC is not approved is refused every service EXCEPT the ones flagged here. This
        /// is the "which options still work while KYC is pending" decision, and it is the admin's
        /// to make — one row per service, set from the KYC Approvals screen.
        /// </summary>
        [DefaultValue(false)]
        public bool IsAllowedWithoutKYC { get; set; }

        /// <summary>
        /// Stays open during a scheduled system-down window. While the platform is down (Key
        /// Settings "System Down", from/to clock times) every financial transaction is refused
        /// EXCEPT on services flagged here - the "a few services may still run" decision the
        /// operator makes per service, set from the Service Catalog.
        ///
        /// Default false, so turning maintenance on stops everything until the operator says
        /// otherwise. A window that silently let a service through would be worse than no window.
        /// </summary>
        [DefaultValue(false)]
        public bool IsAllowedWhenSystemDown { get; set; }

        // Which report this service's transactions belong to (ServiceKinds constant:
        // RECHARGE / DMT / BBPS / PAYOUT / AEPS / MATM / CASHDEPOSIT, or NONE = no report).
        // Empty = auto-detect from Name/Category keywords. Admin sets it from the Service Catalog.
        [Column(TypeName = "varchar(50)")]
        public string ReportKind { get; set; } = string.Empty;
    }
}