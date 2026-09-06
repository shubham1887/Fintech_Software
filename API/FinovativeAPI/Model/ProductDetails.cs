using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProductDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductDetailsID { get; set; }

        public int SubCategoryID { get; set; }

        [ForeignKey("SubCategoryID")]
        public SubCategory SubCategory { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal SellingPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal BuyingPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Discount { get; set; }

        [Required]
        public int TotalStock { get; set; }
    }
}