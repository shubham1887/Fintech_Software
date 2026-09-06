using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OTPMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OTPMasterID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string OTP { get; set; }

        [DefaultValue(false)]
        public bool IsVerified { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

    }
}