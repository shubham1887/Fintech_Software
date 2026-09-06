using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserBankDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBankDetailsID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string BankName { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string AccountNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string IFSC { get; set; }

        [DefaultValue(true)]
        public bool IsOn { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string HolderName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Branch { get; set; }

        public string QRCodeImageURL { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string UPINumber { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }
    }
}