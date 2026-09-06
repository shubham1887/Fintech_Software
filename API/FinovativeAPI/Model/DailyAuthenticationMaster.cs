using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class DailyAuthenticationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthenticationID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int ProviderID { get; set; }
        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int ServiceID { get; set; }
        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }
        public int OperatorID { get; set; }
       
        [Column(TypeName = "varchar(100)")]
        public string Pipe { get; set; }
    }
}