using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DMTSender : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SenderID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string GUID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Address { get; set; } = string.Empty;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; } = null!;

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsOTPVerified { get; set; }

        [DefaultValue(false)]
        public bool IsKYC { get; set; }

        [Column(TypeName = "varchar(2000)")]
        public string KYCDocumentURL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string Pincode { get; set; } = string.Empty;
    }
}
