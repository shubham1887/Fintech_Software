using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderResponseTypeMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderResponseTypeMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Type { get; set; }
    }
}