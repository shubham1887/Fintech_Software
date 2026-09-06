using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class AuditLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditLogID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Module { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string OldValue { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string NewValue { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string AdditionalInfo { get; set; } = string.Empty;
    }
}
