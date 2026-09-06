using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SMSMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SMSMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string SMSString { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string BalanceURL { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string WhatsAppURL { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string APIName { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

        private string _HeaderJson;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodTypeWhatsApp { get; set; }

        private string _HeaderJsonWhatsApp;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJsonWhatsApp
        {
            get => _HeaderJsonWhatsApp.TrimIfNotNull();
            set => _HeaderJsonWhatsApp = value.TrimIfNotNull();
        }

        [Column(TypeName = "varchar(30)")]
        public string MobileWithOrWithout91 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string SMSOTPURL { get; set; }
         
        [Required]
        [DefaultValue(false)]
        public bool IsOTP { get; set; } 
    }
}