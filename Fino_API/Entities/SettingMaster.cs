using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SettingMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsOn { get; set; }

        [DefaultValue(false)]
        public bool IsSecure { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsPassword { get; set; }
    }
}
