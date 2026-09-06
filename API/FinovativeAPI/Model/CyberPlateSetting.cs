using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CyberPlateSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CyberPlateSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [DefaultValue(false)]
        public bool IsActiveForWebsite { get; set; }

        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string AP { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string OP { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string SD { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Password { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string TermID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string AID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string SecretKeyPath { get; set; }
    }
}