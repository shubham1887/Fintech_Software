using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SlabMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlabMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(80)")]
        public string SlabName { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
    }
}
