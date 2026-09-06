using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderResponseTypeMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderResponseTypeMasterID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Type { get; set; } = string.Empty;
    }
}
