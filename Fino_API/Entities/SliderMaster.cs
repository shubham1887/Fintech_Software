using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SliderMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SliderMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string SliderPath { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string Type { get; set; } = string.Empty;
    }
}
