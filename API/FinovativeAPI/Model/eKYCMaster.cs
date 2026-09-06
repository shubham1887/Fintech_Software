using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class eKYCMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int eKYCMasterID { get; set; }

        public int ProviderID { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string OrderID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string OTPGenerateHash { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ChargeAmount { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string OTPGenerateRequest { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string OTPGenerateResponse { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string VerifyRequest { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string VerifyResponse { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string OTPToken { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string KYCToken { get; set; }
       
        [Column(TypeName = "varchar(200)")]
        public string wadh { get; set; }
    }
}