using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UpiBeneficiary : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UpiBeneficiaryID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _UpiID;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string UpiID
        {
            get => _UpiID.TrimIfNotNull();
            set => _UpiID = value.TrimIfNotNull();
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

        public int UpiSenderID { get; set; }

        [ForeignKey("UpiSenderID")]
        public UpiSender Sender { get; set; }

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
