using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DMTSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DMTSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string WalletID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string CallBackURL { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string UserName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal SplitAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SenderLimit { get; set; }

        [DefaultValue(false)]
        public bool IsCCF { get; set; }

        [DefaultValue(false)]
        public bool IsNeft { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsPayoutDMT { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ProviderCode { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsExpressDMT { get; set; }
    }
}
