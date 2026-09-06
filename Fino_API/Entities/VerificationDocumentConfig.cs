using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// Admin-configured document requirement for Manual (non-eKYC) verification. Fully
    /// database-driven so new regulatory documents can be added from the admin panel without
    /// any code change — the Manual Verification screen renders whatever is active here.
    /// </summary>
    public class VerificationDocumentConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VerificationDocumentConfigID { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string DocumentName { get; set; } = string.Empty;

        /// <summary>Stable machine code (e.g. PAN, GST, AADHAAR) used to key the stored user document.</summary>
        [Column(TypeName = "varchar(50)")]
        public string DocumentCode { get; set; } = string.Empty;

        /// <summary>Free-form category (Identity / Business / Address …).</summary>
        [Column(TypeName = "varchar(50)")]
        public string DocumentType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
