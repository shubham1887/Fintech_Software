using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class VerificationSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VerificationSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }      

        [DefaultValue(true)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Token { get; set; }     

        [Column(TypeName = "varchar(500)")]
        public string CallBackURL { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string UserName { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string AuthCode { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; }

        [DefaultValue(false)]
        public bool IsAccountVerify { get; set; }

        [DefaultValue(false)]
        public bool IsUPIVerify { get; set; }

        [DefaultValue(false)]
        public bool IsPayoutVerify { get; set; }
    }
}