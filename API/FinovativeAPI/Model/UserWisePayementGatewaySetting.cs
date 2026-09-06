using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserWisePayementGatewaySetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserWisePayementGatewaySettingID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int PaymentGatewaySettingID { get; set; }

        [ForeignKey("PaymentGatewaySettingID")]
        public PaymentGatewaySetting PaymentGatewaySetting { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }
    }
}