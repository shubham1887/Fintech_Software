using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class WalletMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WalletID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForPaymentGateway { get; set; }

        [DefaultValue(true)]
        public bool IsValidForTransfer { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ShortName { get; set; }

        [DefaultValue(true)]
        public bool IsWalletTransferEnabled { get; set; }

        [DefaultValue(0)]
        public int UISequence { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForStaticQR { get; set; }

        [DefaultValue(true)]
        public bool IsActiveForeCollection { get; set; }

        [DefaultValue(false)]
        public bool IsValidUserCreation { get; set; }
    }
}