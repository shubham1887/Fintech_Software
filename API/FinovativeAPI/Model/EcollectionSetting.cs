using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class EcollectionSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EcollectionSettingID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PrivateKeyPath { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PrivateKeyPassword { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PublicKeyPath { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        public bool IsActive { get; set; }
        public int WalletID { get; set; }

        private string _Prefix;

        [Column(TypeName = "varchar(100)")]
        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _IFSCCode;

        [Column(TypeName = "varchar(20)")]
        public string IFSCCode
        {
            get => _IFSCCode.TrimIfNotNull();
            set => _IFSCCode = value.TrimIfNotNull();
        }
    }
}