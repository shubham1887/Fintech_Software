using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class EmailSettingMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailSettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string HostName { get; set; } = string.Empty;

        [Required]
        public int Port { get; set; }

        public bool IsSSL { get; set; }

        public bool IsActive { get; set; }
    }
}
