using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CashCollectionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashCollectionMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Outstanding { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal MaxOutstanding { get; set; }
    }
}