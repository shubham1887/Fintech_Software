using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CompanyMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; } = string.Empty;
    }
}
