using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CategoryChannelMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryChannelMappingID { get; set; }

        public int ChannelCategoryID { get; set; }

        [ForeignKey("ChannelCategoryID")]
        public ChannelCategory ChannelCategory { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ChannelName { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ChannelImagePath { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}