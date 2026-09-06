using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ServiceWiseSliderSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceWiseSliderSettingID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ImagePath { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ReferenceLink { get; set; }

        public int Priority { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [Column(TypeName = "varchar(10)")]
        [DefaultValue(StaticModel.PlatformCode.App)]
        public string Type { get; set; }
    }
}