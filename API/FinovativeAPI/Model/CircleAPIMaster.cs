using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CircleAPIMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleAPIMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string CircleAPIURL { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}