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
    }
}
