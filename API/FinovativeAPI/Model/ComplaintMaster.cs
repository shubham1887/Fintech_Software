using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ComplaintMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComplaintMasterID
        {
            get;
            set;
        }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster User { get; set; }

        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster Service { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster Operator { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string CustomerRemark { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string AdminRemark { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ComplaintStatus { get; set; }

        public int TransactionMasterDOCInt { get; set; }
        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ComplaintType { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string NewConsumerNumber { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ProviderRequestID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ReferenceNo { get; set; }
    }
}