using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SliderMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SliderMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string SliderPath { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(10)")]
        [DefaultValue(StaticModel.PlatformCode.Web)]
        public string Type { get; set; }

        public bool IsDelete { get; set; }
    }
}