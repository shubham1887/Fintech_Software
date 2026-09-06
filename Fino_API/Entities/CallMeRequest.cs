using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CallMeRequest : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CallMeRequestID { get; set; }

        public int CallMeCategoryMasterID { get; set; }

        [ForeignKey("CallMeCategoryMasterID")]
        public CallMeCategoryMaster CallMeCategoryMaster { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int UplineUserID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string UplineRemark { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; }
    }
}