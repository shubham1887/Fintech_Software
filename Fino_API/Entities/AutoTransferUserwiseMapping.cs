using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class AutoTransferUserwiseMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AutoTransferUserwiseMappingID { get; set; }
        public int ParentID { get; set; }
        public int UserID { get; set; }
        [Column(TypeName = "varchar(10)")] public string OTP { get; set; } = string.Empty;
        public bool IsOTPVerified { get; set; }
        [Column(TypeName = "decimal(18,5)")] public decimal MaxOutstanding { get; set; }
        [Column(TypeName = "decimal(18,5)")] public decimal RechargeLowBalanceReached { get; set; }
        [Column(TypeName = "decimal(18,5)")] public decimal RechargeAmountIncreasedBy { get; set; }
        [Column(TypeName = "decimal(18,5)")] public decimal DMTLowBalanceReached { get; set; }
        [Column(TypeName = "decimal(18,5)")] public decimal DMTAmountIncreasedBy { get; set; }
        public virtual UserMaster? UserMaster { get; set; }
    }
}
