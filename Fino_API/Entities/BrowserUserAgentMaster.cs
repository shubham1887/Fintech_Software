using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class BrowserUserAgentMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BrowserUserAgentMasterID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string UserAgent { get; set; } = string.Empty;
    }
}
