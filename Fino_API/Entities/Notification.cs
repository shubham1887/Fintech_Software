using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class Notification : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string NotificationText { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string NotificationImagePath { get; set; } = string.Empty;
    }
}
