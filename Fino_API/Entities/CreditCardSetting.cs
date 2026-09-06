using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CreditCardSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CreditCardSettingID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Slat { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string PublicKey { get; set; } = string.Empty;

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; } = string.Empty;
    }
}
