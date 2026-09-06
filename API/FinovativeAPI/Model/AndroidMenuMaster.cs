using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class AndroidMenuMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AndroidMenuMasterID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string MenuName { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Key { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string URL { get; set; }

        public int UserTypeID { get; set; }

        public int Sequence { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string SectionName { get; set; }

        public bool IsActive { get; set; }
    }
}