using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SubCategory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubCategoryID { get; set; }

        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string SubCategoryName { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ShortDescription { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string LongDescription { get; set; }
    }
}