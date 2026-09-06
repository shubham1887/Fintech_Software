using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderUserMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderUserMappingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Key { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; }

        public int ProviderID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }
    }
}
