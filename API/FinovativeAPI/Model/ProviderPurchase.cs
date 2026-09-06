using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderPurchase : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderPurchaseID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; }

        public DateTime PurchaseDate { get; set; }
        public int PurchaseDOCInt { get; set; }

        public bool IsDelete { get; set; }
    }
}