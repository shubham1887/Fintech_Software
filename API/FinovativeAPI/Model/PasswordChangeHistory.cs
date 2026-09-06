using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PasswordChangeHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PasswordChangeHistoryID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string IPAddress { get; set; }
    }
}