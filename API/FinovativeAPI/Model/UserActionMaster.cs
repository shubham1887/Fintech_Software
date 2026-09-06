using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserActionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserActionMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Platform { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string FingurePrint { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; }
    }
}