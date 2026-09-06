using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BrowserUserAgentMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BrowserUserAgentMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string UserAgent { get; set; }
    }
}