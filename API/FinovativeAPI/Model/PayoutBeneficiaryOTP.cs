using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PayoutBeneficiaryOTP : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutBeneficiaryOTPID { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string OTP { get; set; }

        public int PayoutBeneficaryID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public bool IsOTPVerified { get; set; }
    }
}