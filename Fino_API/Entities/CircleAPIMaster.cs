using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CircleAPIMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleAPIMasterID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string CircleAPIURL { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
