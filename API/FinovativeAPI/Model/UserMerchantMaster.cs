using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserMerchantMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserMerchantMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PaySprintUPIMerchantCode { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? PaySprintStaticQR { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? PaySprintUPIRefid { get; set; }
    }
}