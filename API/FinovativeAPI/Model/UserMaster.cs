using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        private string _FirmName;

        [Required]
        [DataType("varchar(100)")]
        public string FirmName
        {
            get => _FirmName;
            set => _FirmName = value;
        }

        [Required]
        [StringLength(20, ErrorMessage = "Mobile number must be of 10 digit.")]
        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Password { get; set; }

        [Required]
        [Column(TypeName = "varchar(70)")]
        public string EmailID { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string GSTNo { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string PANNo { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CIN { get; set; }

        [Column(TypeName = "varchar(16)")]
        public string AadharNo { get; set; }

        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public int ParentID { get; set; }

        private string _Address;

        [Column(TypeName = "varchar(max)")]
        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        public bool IsInitialPassword { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string UniqueReferenceNo { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")]
        public int Mpin { get; set; }

        public bool IsKYCApproved { get; set; }

        public bool IsLoginOTP { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AppFCMToken { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string WebFCMToken { get; set; }

        public int UserTypeID { get; set; }

        [ForeignKey("UserTypeID")]
        public UserTypeMaster UserType { get; set; }

        public int StateID { get; set; }

        [ForeignKey("StateID")]
        public StateMaster State { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string APIToken { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string UDD { get; set; }

        [DefaultValue(true)]
        public bool IsWeb { get; set; }

        [DefaultValue(true)]
        public bool IsApp { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ProfileImagePath { get; set; }

        [DefaultValue(true)]
        public bool IsIPSecure { get; set; }

        public bool AllowRealAPI { get; set; }

        [DefaultValue(false)]
        public bool IsChannel { get; set; }

        public bool IsReverseRechargeApplicable { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string VirtualAccountNumber { get; set; }

        [DefaultValue(true)]
        public bool IsInvoiceApplicable { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string PersonName { get; set; }

        public int JoiningByUserID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RegistrationType { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string InstantPayOutletID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Comm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string CommType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string CommissionCategory { get; set; }

        [DefaultValue(false)]
        public bool IsTwoFA { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string TwoFASecretKey { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CityName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Pincode { get; set; }

        public int WalletIDforEcollection { get; set; }

        [DefaultValue(1)]
        public int StaticQRWalletID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string AliasName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MahagramAEPSAgentID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MahagramBBPSAgentID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PaysprintAEPSAgentID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string EkoAgentID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string FingpayAgentID { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string ApiClubAgentID { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string ICICI_Static_GUID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ResponseType { get; set; }

        [DefaultValue(10)]
        public int PayoutAccountCount { get; set; }

        [DefaultValue(false)]
        public bool IsPin { get; set; }

        [StringLength(21, ErrorMessage = "WhatsApp Number must be of 10 digit.")]
        [Column(TypeName = "varchar(21)")]
        public string WhatsAppNumber { get; set; }

        [DefaultValue(false)]
        public bool NumberFirst { get; set; }

        [DefaultValue("Monthly")]
        [Column(TypeName = "varchar(50)")]
        public string GSTHoldCutting { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PayOneMoneyAgentCode { get; set; }

        [DefaultValue("Inclusion")]
        [Column(TypeName = "varchar(100)")]
        public string GSTPrinciple { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string EzyPayRetailerID { get; set; }

        [Column(TypeName = "varchar(5)")]
        public string ReferralCode { get; set; }

        [DefaultValue(0)]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ReferralAmount { get; set; }

        [DefaultValue(0)]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ReferralBenefitMinAmount { get; set; }

        [DefaultValue(false)]
        public bool IseKYC { get; set; }

        [DefaultValue(false)]
        public bool IsCommissionHold { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string BankITAgentCode { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CredoPay_LoginID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CredoPay_Password { get; set; }

        [DefaultValue(false)]
        public bool IsReferralPopup { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MorePayMerchantCode { get; set; }

        [DefaultValue(false)]
        public bool IsMorePayEKYC { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RoyalPayAgentCode { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RMGPayAgentCode { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string ShopAddress { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string NPCIAgentID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PaySwiffMerchantId { get; set; }

    }
}