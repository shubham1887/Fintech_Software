using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class SignUp:BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SignUpID { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string MobileNumber { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string PANNo { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ReferralCode { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string EmployeeID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string OrderID { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string ProfileID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string RedirectURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string PinCode { get; set; }

    }
}
