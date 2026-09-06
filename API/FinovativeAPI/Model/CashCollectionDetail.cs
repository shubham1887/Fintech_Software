using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CashCollectionDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashCollectionDetailID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ParentID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingOpening { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingClosing { get; set; }

        private string _ParentRemark;

        [Column(TypeName = "varchar(100)")]
        public string ParentRemark
        {
            get => _ParentRemark.TrimIfNotNull();
            set => _ParentRemark = value.TrimIfNotNull();
        }

        private string _PayMode;

        [Column(TypeName = "varchar(100)")]
        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _Credit;

        [Column(TypeName = "varchar(50)")]
        public string Credit
        {
            get => _Credit.TrimIfNotNull();
            set => _Credit = value.TrimIfNotNull();
        }

        private string _Debit;

        [Column(TypeName = "varchar(50)")]
        public string Debit
        {
            get => _Debit.TrimIfNotNull();
            set => _Debit = value.TrimIfNotNull();
        }

        [Column(TypeName = "varchar(100)")]
        public string UTRNumber { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}