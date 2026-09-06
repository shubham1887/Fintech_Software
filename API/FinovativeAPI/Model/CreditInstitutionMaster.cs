using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CreditInstitutionMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CreditInstitutionMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string BranchID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string AccountNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string EncryptionKey { get; set; }

        public bool IsActive { get; set; }

        public bool IsActiveWeb { get; set; }

    }
}
