using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class GSTHold : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GSTHoldID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal HoldAmount { get; set; }

        public DateTime GSTUploadedDate { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string ReceiptPath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string GSTUploadRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string AdminRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string GSTNo { get; set; } = string.Empty;

        public int StateID { get; set; }

        [ForeignKey("StateID")]
        public virtual StateMaster? StateGst { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal TaxableValue { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal IGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal CGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SGST { get; set; }
    }
}
