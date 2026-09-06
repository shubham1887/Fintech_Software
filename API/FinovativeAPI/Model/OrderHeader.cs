using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OrderHeader : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderHeaderID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string DeliveryAddress { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string OrderPhoneNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string OrderEmail { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string OrderStatus { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string PaymentStatus { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string InvoiceNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal TotalAmount { get; set; }
    }
}