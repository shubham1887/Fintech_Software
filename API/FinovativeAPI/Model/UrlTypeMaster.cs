using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UrlTypeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UrlTypeID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UrlType { get; set; }
    }
}