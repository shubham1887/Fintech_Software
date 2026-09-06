using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CashFreeBeneficiary : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashFreeBeneficiaryID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _BankName;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _AccountNo;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _IFSC;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _GUID;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string GUID
        {
            get => _GUID.TrimIfNotNull();
            set => _GUID = value.TrimIfNotNull();
        }

        private string _SenderMobile;

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string SenderMobile
        {
            get => _SenderMobile.TrimIfNotNull();
            set => _SenderMobile = value.TrimIfNotNull();
        }

        private string _IPAddress;

        [Column(TypeName = "varchar(500)")]
        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        public int SenderID { get; set; }

        [ForeignKey("SenderID")]
        public Sender Sender { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [DefaultValue(false)]
        public bool IsOTPVerified { get; set; }

        [DefaultValue(false)]
        public bool IsNameVerified { get; set; }
    }
}