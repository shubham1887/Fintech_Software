using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One account in the chart of accounts. Code is the stable key the posting rules and the
    /// integrity checks reference — never the id, which differs per install.
    /// </summary>
    public class AcctAccount : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctAccountID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Asset / Liability / Income / Expense / Equity. Fixes the normal balance.</summary>
        [Column(TypeName = "varchar(20)")]
        public string AccountType { get; set; } = string.Empty;

        /// <summary>Dr or Cr — stored rather than always derived from the type, so a contra account
        /// (an asset that normally carries a credit, say) can be set without special-casing.</summary>
        [Column(TypeName = "varchar(2)")]
        public string NormalBalance { get; set; } = string.Empty;

        /// <summary>A control account is tied out against an operational table by a daily check.</summary>
        public bool IsControl { get; set; }

        public int? ParentAccountID { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
