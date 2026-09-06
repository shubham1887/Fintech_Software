using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BeneficiaryOTP : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BeneficiaryOTPID { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string SenderMobile { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string OTP { get; set; }

        public int SenderID { get; set; }

        [ForeignKey("SenderID")]
        public Sender Sender { get; set; }

        public int BeneficiaryID { get; set; }

        [ForeignKey("BeneficiaryID")]
        public Beneficiary Beneficiary { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public bool IsOTPVerified { get; set; }
    }
}