using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DMTBeneficiary : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BeneficiaryID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string AccountNo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string IFSC { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string GUID { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string SenderMobile { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string IPAddress { get; set; } = string.Empty;

        public int SenderID { get; set; }

        [ForeignKey("SenderID")]
        public DMTSender DMTSender { get; set; } = null!;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; } = null!;

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsOTPVerified { get; set; }

        [DefaultValue(false)]
        public bool IsNameVerified { get; set; }
    }
}
