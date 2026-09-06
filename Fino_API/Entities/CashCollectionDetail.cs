using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CashCollectionDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashCollectionDetailID { get; set; }

        public int UserID { get; set; }
        public int ParentID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingOpening { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingClosing { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ParentRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string PayMode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Credit { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Debit { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string UTRNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; } = string.Empty;

        public bool IsDelete { get; set; }
    }
}
