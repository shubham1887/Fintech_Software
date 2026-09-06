using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class VirtualBalance : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VirtualBalanceID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int WalletID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string DOCType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }
    }
}