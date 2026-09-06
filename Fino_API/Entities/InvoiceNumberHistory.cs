using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class InvoiceNumberHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceNumberHistoryID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ServiceCategory { get; set; } = string.Empty;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "varchar(16)")]
        public string InvoiceNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(16)")]
        public string InvoiceType { get; set; } = string.Empty;

        public int IncrementValue { get; set; }
    }
}
