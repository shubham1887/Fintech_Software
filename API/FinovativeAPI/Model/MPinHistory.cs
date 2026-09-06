using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class MPinHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MPinHistoryID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")]
        public int Mpin { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DeviceID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Device { get; set; }

    }
}
