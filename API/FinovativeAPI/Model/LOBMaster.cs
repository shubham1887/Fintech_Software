using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class LOBMaster :BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LOBMasterID { get; set; }
        [Required]
        [Column(TypeName = "varchar(150)")]
        public string LineOfBusiness { get; set; }
        [Required]
        [Column(TypeName = "varchar(150)")]
        public string LOBCode { get; set; }
    }
}
