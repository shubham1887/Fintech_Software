using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PayoutBeneficiary : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutBeneficaryID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string AccountNo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string IFSC { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string AdminRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string? Document { get; set; }

        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsNameVerified { get; set; }
    }
}
