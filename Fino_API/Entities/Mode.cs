using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    // Fund-request Mode master — the method of a fund transfer (Bank, Cash Deposit, Cash Collection,
    // Office, Credit, UPI, QR Payment, Wallet, Others). Each Mode owns one or more SubModes that name
    // the actual source/destination (e.g. Bank → HDFC / ICICI / SBI). Fully admin-managed, so new
    // payment methods are a data change with no code deployment.
    public class Mode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ModeID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ModeName { get; set; } = string.Empty;

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
