using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    // Fund-request SubMode master — a concrete source/destination under a Mode (e.g. Bank → HDFC Bank,
    // QR Payment → PhonePe QR, Credit → Temporary Credit). Carries an optional running Balance so the
    // admin can track how much has been collected/settled against each account.
    public class SubMode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubModeID { get; set; }

        [Required]
        public int ModeID { get; set; }

        [ForeignKey("ModeID")]
        public Mode Mode { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string SubModeName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }

        // Source/destination details. Which ones are relevant depends on the parent Mode
        // (Bank → BankName/IFSC/BeneficiaryName/AccountNumber; QR Payment → QRCode/QRImage);
        // all optional so any Mode can use whichever apply.
        [Column(TypeName = "varchar(150)")]
        public string BankName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string IFSC { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string BeneficiaryName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string AccountNumber { get; set; } = string.Empty;

        // UPI id / QR string encoded in the QR.
        [Column(TypeName = "varchar(200)")]
        public string QRCode { get; set; } = string.Empty;

        // Uploaded QR image path (served from wwwroot/uploads via Upload/Image).
        [Column(TypeName = "varchar(500)")]
        public string QRImage { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int UpdatedBy { get; set; }
    }
}
