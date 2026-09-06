using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class IPMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IpMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string IP { get; set; } = string.Empty;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; } = string.Empty;
    }
}
