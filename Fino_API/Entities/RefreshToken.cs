using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class RefreshToken : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RefreshTokenID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public UserMaster User { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string JwtId { get; set; } = string.Empty;

        public DateTime ExpiryDateTime { get; set; }

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string UserAgent { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string DeviceType { get; set; } = string.Empty;
    }
}
