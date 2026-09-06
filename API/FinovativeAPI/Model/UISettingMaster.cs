using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UISettingMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UISettingMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Key { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Value { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }
    }
}