using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OrderDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderDetailsID { get; set; }

        public int OrderHeaderID { get; set; }

        [ForeignKey("OrderHeaderID")]
        public OrderHeader OrderHeader { get; set; }

        public int ProductDetailsID { get; set; }

        [ForeignKey("ProductDetailsID")]
        public ProductDetails ProductDetails { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Rate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal CGST { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal SGST { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal GST { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Discount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal TotalAmount { get; set; }
    }
}