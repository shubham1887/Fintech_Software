using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class InvoiceNumberHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceNumberHistoryID { get; set; }

        private string _ServiceCategory;

        [Column(TypeName = "varchar(50)")]
        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull(); set => _ServiceCategory = value.TrimIfNotNull();
        }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [Column(TypeName = "varchar(16)")]
        public string InvoiceNo { get; set; }

        [Column(TypeName = "varchar(16)")]
        public string InvoiceType { get; set; }

        public int IncrementValue { get; set; }
    }
}