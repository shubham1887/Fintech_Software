using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PaymentGatewaySetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentGatewaySettingID { get; set; }

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

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string SuccessURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ActionURL { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string StatusCheckURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string FailURL { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [DefaultValue(false)]
        public bool IsUPI { get; set; }
       
        [DefaultValue(false)]
        public bool IsManualStatusChange { get; set; }
        public int SettlementHoldInMinutes { get; set; }
    }
}