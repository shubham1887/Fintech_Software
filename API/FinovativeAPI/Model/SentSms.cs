using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SentSMS : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SentSMSID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Mobile { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Message { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        public int SMSMasterID { set; get; }

        [Column(TypeName = "varchar(100)")]
        public string TemplateID { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string WhatsAppNumber { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Request { get; set; }
    }
}