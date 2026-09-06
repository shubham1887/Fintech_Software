using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OverdraftDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OverdraftDetailID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster User { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster Wallet { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OverdraftOpening { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OverdraftAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OverdraftClosing { get; set; }

        private string _UserRemark;

        [Column(TypeName = "varchar(100)")]
        public string UserRemark
        {
            get => _UserRemark.TrimIfNotNull();
            set => _UserRemark = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        [Column(TypeName = "varchar(100)")]
        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningWalletBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingWalletBalance { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; }
    }
}