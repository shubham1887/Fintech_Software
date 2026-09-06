using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserChannelSlabMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserChannelSlabMappingID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int ChannelSlabMasterID { get; set; }

        [ForeignKey("ChannelSlabMasterID")]
        public ChannelSlabMaster ChannelSlabMaster { get; set; }

        public bool IsDelete { get; set; }
        public int UplineUserID { get; set; }
    }
}