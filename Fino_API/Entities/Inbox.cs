using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class Inbox : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboxID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Through { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string Param1 { get; set; } = string.Empty;
    }
}
