using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class WrongPasswordLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long WrongPasswordLogID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string WrongPassword { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LogAction { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string UserAgent { get; set; } = string.Empty;

        public int AttemptCount { get; set; } = 1;
    }
}
