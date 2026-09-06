using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PureDMTTransferBeneDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PureDMTTransferBeneDetailID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string SenderNumber { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string BeneID { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string AccountNumber { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string VerifyBeneName { get; set; }

        public bool IsVerify { get; set; }
    }
}