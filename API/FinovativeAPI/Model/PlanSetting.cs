using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PlanSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlanSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; }
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
       
        [Column(TypeName = "varchar(200)")]
        public string Circle { get; set; }

        public string Password { get; set; }

    }
}