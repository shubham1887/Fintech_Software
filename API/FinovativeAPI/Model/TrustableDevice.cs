using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TrustableDevice : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsVerified { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Browser { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Device { get; set; }
    }
}