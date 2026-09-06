using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DenominationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DenominationMasterID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string DenominationRange { get; set; } = string.Empty;

        public bool IsStop { get; set; }
    }
}
