using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserSlabMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSlabMappingID { get; set; }

        public bool IsDelete { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int SlabMasterID { get; set; }

        [ForeignKey("SlabMasterID")]
        public SlabMaster SlabMaster { get; set; }
    }
}