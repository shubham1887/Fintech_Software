using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PayoutBeneficiary : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutBeneficaryID { get; set; }

        private string _AccountHolderName;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string AccountHolderName
        {
            get => _AccountHolderName.TrimIfNotNull();
            set => _AccountHolderName = value.TrimIfNotNull();
        }

        private string _BankName;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
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

        private string _Status;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }
        [Column(TypeName = "varchar(500)")]
        public string Document { get; set; }

        public int UserID { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [DefaultValue(false)]
        public bool IsOTPVerified { get; set; }

        [DefaultValue(false)]
        public bool IsNameVerified { get; set; }
    }
}