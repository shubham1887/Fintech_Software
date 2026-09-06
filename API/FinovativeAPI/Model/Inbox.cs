using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class Inbox : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboxID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Through { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string IPAddress { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Param1 { get; set; }
    }
}