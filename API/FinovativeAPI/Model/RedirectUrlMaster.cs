using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class RedirectUrlMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RedirectUrlMasterID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string RedirectUrl { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}