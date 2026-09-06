using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SentSMS : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SentSMSID { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Mobile { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Message { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; } = string.Empty;

        public int SMSMasterID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string TemplateID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string WhatsAppNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Request { get; set; } = string.Empty;
    }
}
