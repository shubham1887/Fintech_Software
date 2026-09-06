using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SmsMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SMSMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string SMSString { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string BalanceURL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string WhatsAppURL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string APIName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodTypeWhatsApp { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJsonWhatsApp { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string MobileWithOrWithout91 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string SMSOTPURL { get; set; } = string.Empty;

        [Required]
        [DefaultValue(false)]
        public bool IsOTP { get; set; }
    }
}
