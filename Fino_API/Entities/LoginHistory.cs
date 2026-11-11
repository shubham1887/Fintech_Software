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

        /// <summary>
        /// Where the device said it was, in decimal degrees. Kept as text because that is what
        /// every client sends and because a blank here has to mean "not supplied" rather than
        /// "0,0", which is a real place in the Atlantic.
        /// </summary>
        [Column(TypeName = "varchar(32)")]
        public string Latitude { get; set; } = string.Empty;

        [Column(TypeName = "varchar(32)")]
        public string Longitude { get; set; } = string.Empty;
    }
}
