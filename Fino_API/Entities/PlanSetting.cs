using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PlanSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlanSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string UserID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; } = string.Empty;

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Circle { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
