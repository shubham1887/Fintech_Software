using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class WebHookSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WebHookSettingID { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string URL { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson { get; set; }
    }
}
