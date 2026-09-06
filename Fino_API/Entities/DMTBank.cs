using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DMTBank : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DMTBankID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string DefaultIFSC { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [DefaultValue(true)]
        public bool IsIMPS { get; set; } = true;
    }
}
