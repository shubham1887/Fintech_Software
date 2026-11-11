using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{

    public class UserMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserMasterID { get; set; }

        public int? ParentID { get; set; }

        [Required]
        public int? UserTypeID { get; set; }

        [ForeignKey(nameof(UserTypeID))]
        public UserTypeMaster UserType { get; set; }
        
        [Column(TypeName = "varchar(200)")]
        public string PersonName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FirmName { get; set; }

        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string PasswordHash { get; set; }

        [Required]
        [Column(TypeName = "varchar(70)")]
        public string EmailID { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string GSTNo { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string PANNo { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CIN { get; set; } = string.Empty;

        [Column(TypeName = "varchar(16)")]
        public string AadharNo { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Address { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int Mpin { get; set; }

        // The transaction/app-open MPIN. Stored hashed (never the digits) — the legacy int Mpin
        // above is left untouched for back-compat but is no longer the source of truth. A user
        // opts in with IsMpinEnabled; the platform-wide requirement is the IsMpinEnabled key in
        // SettingMaster (Key Setting), which the client reads to decide when to prompt.
        [Column(TypeName = "varchar(128)")]
        public string MpinHash { get; set; } = string.Empty;

        public bool IsMpinEnabled { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string UniqueReferenceNo { get; set; }= string.Empty;

        public bool IsKYCApproved { get; set; }
        public bool IsLoginOTP { get; set; }

        public int? StateID { get; set; }

        [ForeignKey(nameof(StateID))]
        public StateMaster State { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string APIToken { get; set; } = string.Empty;

        // API-user IP security: when on, only requests from the whitelisted IP(s) are accepted.
        // WhitelistedIPs holds one or more IPs (newline / comma separated).
        public bool IsIPSecure { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string WhitelistedIPs { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(500)")]
        public string ProfileImagePath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string VirtualAccountNumber { get; set; } = string.Empty;

        public bool IsInvoiceApplicable { get; set; } = true;

        public int? JoiningByUserID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RegistrationType { get; set; }

        public bool IsTwoFA { get; set; } = false;

        [Column(TypeName = "varchar(100)")]
        public string CityName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Pincode { get; set; }

        public int? WalletIDforEcollection { get; set; }
        public int StaticQRWalletID { get; set; } = 1;

        [Column(TypeName = "varchar(100)")]
        public string AliasName { get; set; }

        public bool IsPin { get; set; } = false;

        [StringLength(21)]
        [Column(TypeName = "varchar(21)")]
        public string WhatsAppNumber { get; set; }


        [Column(TypeName = "varchar(5)")]
        public string ReferralCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ReferralAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ReferralBenefitMinAmount { get; set; } = 0;

        public bool IseKYC { get; set; } = false;
        public bool IsCommissionHold { get; set; } = false;
        public bool IsInitialPassword { get; set; } = false;

        [Column(TypeName = "varchar(max)")]
        public string ShopAddress { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string BusinessGSTStateCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string PartnerConfigJson { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string AppFCMToken { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string WebFCMToken { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string NPCIAgentID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string PaySwiffMerchantId { get; set; } = string.Empty;

        /// <summary>
        /// Permanent-free from the subscription requirement. Set on a user directly, and it
        /// cascades down the hierarchy: if a Distributor / Super-Distributor is exempt, every
        /// user under them is effectively exempt too (evaluated by walking the ParentID chain).
        /// </summary>
        public bool IsSubscriptionExempt { get; set; } = false;

        /// <summary>
        /// The account this one is a second role of — see LinkedProfilePolicy.
        ///
        /// One person, one mobile number, two roles. A distributor who also runs a counter, or an
        /// API client who also needs a retailer login to test with, used to need a second mobile
        /// number, because this column did not exist and MobileNumber was unique on its own. The
        /// primary account carries NULL; the second account points at it. Both sign in with the
        /// primary's password and then choose which one to work as.
        ///
        /// This is NOT a hierarchy link — ParentID still places the account in the network, and
        /// the two are separate users in every other respect: own wallet, own slab, own ledger,
        /// own user code. Nothing is shared except the phone number and the credentials.
        /// </summary>
        public int? LinkedPrimaryUserID { get; set; }

        /// <summary>
        /// Generation number of this user's live sessions. Every access token carries the value
        /// that was current when it was minted, and a token whose generation is older than this
        /// one is refused (see JwtAuthorizationAttribute).
        ///
        /// Access tokens are self-contained JWTs, so until this existed the only thing signing out
        /// revoked was the REFRESH token - the access token itself kept working for the rest of
        /// its lifetime, in every tab and on every device holding a copy. Logging out of one tab
        /// therefore left the other tab fully able to transact, which is the half of the
        /// multi-tab problem the browser cannot fix on its own.
        ///
        /// That lifetime is a DAY under the shipped settings (JwtSettings:ExpiryInMinutes), not
        /// the hour the code used to fall back to, so this is the only thing standing between a
        /// copied token and a day of use.
        ///
        /// Bumping it ends every session the user has at once, which is what "log me out" has to
        /// mean on a system that moves money. A password change bumps it for the same reason.
        /// </summary>
        public int SessionEpoch { get; set; } = 0;
    }
}
