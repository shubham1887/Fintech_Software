using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ZoneMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ZoneMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(300)")]
        public string Name { get; set; }
    }
}
