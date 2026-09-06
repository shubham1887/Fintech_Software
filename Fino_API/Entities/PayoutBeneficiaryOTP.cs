using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PayoutBeneficiaryOTP : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutBeneficiaryOTPID { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string OTP { get; set; } = string.Empty;

        public int PayoutBeneficaryID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public bool IsOTPVerified { get; set; }
    }
}
