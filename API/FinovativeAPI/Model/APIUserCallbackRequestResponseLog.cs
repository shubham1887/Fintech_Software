using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class APIUserCallbackRequestResponseLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int APIUserCallbackRequestResponseLogID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Request { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        public int TransactionMasterID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string APIUserRequestID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ConsumerNumber { get; set; }
    }
}