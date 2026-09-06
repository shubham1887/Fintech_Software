using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class MessageSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsSMS { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsMail { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsWhatsUp { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string WhatsUpMessage { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string SmsMessage { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string EmailMessage { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string TemplateID { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsOTP { get; set; }
    }
}