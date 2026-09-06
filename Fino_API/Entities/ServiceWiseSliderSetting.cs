using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ServiceWiseSliderSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceWiseSliderSettingID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ImagePath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string ReferenceLink { get; set; } = string.Empty;

        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Type { get; set; } = string.Empty;
    }
}
