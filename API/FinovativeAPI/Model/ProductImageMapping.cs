using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProductImageMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductImageMappingID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string ProductImageName { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ProductImagePath { get; set; }
    }
}