using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Project.Model
{
    public class BBPSSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BBPSSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string AgentID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PartnerID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string MerchantID { get; set; }
    }
}
