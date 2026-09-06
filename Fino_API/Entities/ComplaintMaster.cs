using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ComplaintMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComplaintMasterID { get; set; }

        public int UserID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string CustomerRemark { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string AdminRemark { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ComplaintStatus { get; set; } = string.Empty;

        public int TransactionMasterDOCInt { get; set; }
        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? ComplaintType { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? NewConsumerNumber { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? ProviderRequestID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? ReferenceNo { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
        public virtual ServiceMaster? ServiceMaster { get; set; }
        public virtual OperatorMaster? OperatorMaster { get; set; }
    }
}
