using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class AutoTransferUserwiseMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AutoTransferUserwiseMappingID { get; set; }

        public int ParentID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string OTP { get; set; }

        [DefaultValue(false)]
        public bool IsOTPVerified { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal MaxOutstanding { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal RechargeLowBalanceReached { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal RechargeAmountIncreasedBy { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal DMTLowBalanceReached { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal DMTAmountIncreasedBy { get; set; }
    }
}