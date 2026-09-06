using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class MessageSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsSMS { get; set; }

        [DefaultValue(true)]
        public bool IsMail { get; set; }

        [DefaultValue(false)]
        public bool IsWhatsUp { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string WhatsUpMessage { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string SmsMessage { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string EmailMessage { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string TemplateID { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsOTP { get; set; }
    }
}
