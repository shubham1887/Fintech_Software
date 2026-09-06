using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class DthPackageChannelMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DthPackageChannelMappingID { get; set; }

        public int OfflineProductMasterID { get; set; }

        [ForeignKey("OfflineProductMasterID")]
        public OfflineProductMaster OfflineProductMaster { get; set; }

        public int CategoryChannelMappingID { get; set; }

        [ForeignKey("CategoryChannelMappingID")]
        public CategoryChannelMapping CategoryChannelMapping { get; set; }

        public int ChannelCategoryID { get; set; }

        [ForeignKey("ChannelCategoryID")]
        public ChannelCategory ChannelCategory { get; set; }

        public bool IsActive { get; set; }
    }
}