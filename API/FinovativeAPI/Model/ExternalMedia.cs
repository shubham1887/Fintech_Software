using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ExternalMedia : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ExternalMediaID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Key { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Value { get; set; }
       
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}