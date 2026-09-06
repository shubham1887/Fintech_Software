using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class RedirectUrlMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RedirectUrlMasterID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string RedirectUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
