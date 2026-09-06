using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class MediaMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaMasterID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MediaType { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string MediaPath { get; set; }
    }
}