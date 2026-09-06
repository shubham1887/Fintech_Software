using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class EcollectionSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EcollectionSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PrivateKeyPath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string PrivateKeyPassword { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string PublicKeyPath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public int WalletID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Prefix { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string IFSCCode { get; set; } = string.Empty;
    }
}
