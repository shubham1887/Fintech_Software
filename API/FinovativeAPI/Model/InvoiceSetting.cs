using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class InvoiceSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string CompanyName { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string LogoURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string CompanyPrefix { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Address { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ShortAddress { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string PinCode { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string District { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Country { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string GSTIN { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string PAN { get; set; }

        [Required]
        [Column(TypeName = "varchar(25)")]
        public string CIN { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string State { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AccountNo { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string AccountType { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string IFSC { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AccountHolderName { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string BankName { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string BranchName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string WebsiteName { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Note { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string FullAddress { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string SignatureURL { get; set; }
    }
}