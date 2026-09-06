using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class MenuActionMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuActionMappingID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Controller { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Action { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MenuMasterGUID { get; set; }
    }
}