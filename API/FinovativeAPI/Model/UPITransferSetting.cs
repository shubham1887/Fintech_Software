using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UPITransferSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UPITransferSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string CallBackURL { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string UserName { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SenderLimit { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinAmount { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AuthCode { get; set; }

        [DefaultValue(false)]
        public bool IsCommonUPI { get; set; }

        [DefaultValue(false)]
        public bool IsUPITransferWithSender { get; set; }
    }
}