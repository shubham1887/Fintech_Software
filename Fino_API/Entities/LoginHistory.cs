using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LoginHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginHistoryID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string IPAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Through { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string LoginMessage { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string SimCardDetails { get; set; } = string.Empty;
    }
}
