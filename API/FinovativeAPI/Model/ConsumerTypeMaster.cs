using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class ConsumerTypeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsumerTypeMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerType { get; set; }
    }
}
