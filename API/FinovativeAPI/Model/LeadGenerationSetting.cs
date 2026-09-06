using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LeadGenerationSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeadGenerationSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Slat { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PublicKey { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }
    }
}