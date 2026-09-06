using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class AccountOpenSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountOpenSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string UserName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string CallBackURL { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Mode { get; set; }
    }
}