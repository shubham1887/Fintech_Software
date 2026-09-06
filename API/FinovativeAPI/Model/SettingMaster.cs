using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SettingMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Key { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; }

        [DefaultValue(true)]
        public bool IsOn { get; set; }

        [DefaultValue(false)]
        public bool IsSecure { get; set; }

        [Column(TypeName = "varchar(max)")]
        [DefaultValue("")]
        public string Description { get; set; }

        [DefaultValue(false)]
        public bool IsPassword { get; set; }
    }
}