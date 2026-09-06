using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PayoutSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutSettingID { get; set; }

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

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ActionURL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string? StatusCheckURL { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? Mode { get; set; }
    }
}
