using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserChannelSlabMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserChannelSlabMappingID { get; set; }

        public int UserID { get; set; }

        public int ChannelSlabMasterID { get; set; }

        public int UplineUserID { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
        public virtual ChannelSlabMaster? ChannelSlabMaster { get; set; }
    }
}
