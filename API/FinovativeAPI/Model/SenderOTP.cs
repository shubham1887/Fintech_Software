using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SenderOTP : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SenderOTPID { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Mobile { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string OTP { get; set; }

        public int SenderID { get; set; }

        [ForeignKey("SenderID")]
        public Sender Sender { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public bool IsOTPVerified { get; set; }
    }
}