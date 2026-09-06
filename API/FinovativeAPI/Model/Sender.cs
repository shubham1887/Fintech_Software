using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class Sender : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SenderID { get; set; }

        private string _Name;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Mobile { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string GUID { get; set; }

        private string _Address;

        [Column(TypeName = "varchar(500)")]
        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public bool IsActive { get; set; }

        public bool IsOTPVerified { get; set; }

        public bool IsKYC { get; set; }

        [Column(TypeName = "varchar(2000)")]
        public string KYCDocumentURL { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Pincode { get; set; }
    }
}