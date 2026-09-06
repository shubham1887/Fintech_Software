using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SlabMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlabMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(80)")]
        public string SlabName { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}