using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UISettingMaster : BaseEntity  // ← BaseEntity add kiya
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UISettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Value { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; } = string.Empty;
    }
}
