using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ExternalMedia : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExternalMediaID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Key { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Value { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
