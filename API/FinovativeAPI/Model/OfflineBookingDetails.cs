using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OfflineBookingDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OfflineBookingDetailsID { get; set; }

        public int OfflineProductMasterID { get; set; }

        [ForeignKey("OfflineProductMasterID")]
        public OfflineProductMaster OfflineProductMaster { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; }

        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Status { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        public int TransactionMasterID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PersonName { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Pincode { get; set; }
    }
}