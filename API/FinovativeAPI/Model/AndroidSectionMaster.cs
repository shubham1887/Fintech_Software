using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class AndroidSectionMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AndroidSectionMasterID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string SectionName { get; set; }
    }
}