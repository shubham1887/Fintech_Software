using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PaymentGatewayLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string GatewayName { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Request { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string OrderID { get; set; }
    }
}