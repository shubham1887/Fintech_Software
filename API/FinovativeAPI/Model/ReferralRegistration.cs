using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ReferralRegistration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReferralRegistrationID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Mobile number must be of 10 digit.")]
        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; }

        [Column(TypeName = "varchar(5)")]
        public string ReferralCode { get; set; }

        public int CreatedByUserID { get; set; }

        [DefaultValue(false)]
        public bool IsBenefitUtilize { get; set; }

        [DefaultValue(0)]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ReferralAmount { get; set; }
    }
}