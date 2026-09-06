using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class AEPSSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AEPSSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Slat { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string PublicKey { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsActiveWeb { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; } = string.Empty;

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsMerchantVerify { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
