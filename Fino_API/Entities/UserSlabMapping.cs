using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserSlabMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSlabMappingID { get; set; }

        public int UserID { get; set; }

        public int SlabMasterID { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
        public virtual SlabMaster? SlabMaster { get; set; }
    }
}
