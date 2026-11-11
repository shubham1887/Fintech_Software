using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// How an event becomes a posting. One rule row is one balanced pair: debit one account and
    /// credit another by the same amount, read from a named field of the event payload. An event
    /// with more than one leg (a sale and its tax) has more than one rule row, and because each row
    /// is a balanced pair the whole voucher balances without the engine having to check arithmetic.
    ///
    /// Rules are data, not code, because the chart differs per install and a reseller may re-map an
    /// event to its own accounts. The accounts are referenced by CODE, resolved to the install's
    /// own account ids at post time.
    /// </summary>
    public class AcctPostingRule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctPostingRuleID { get; set; }

        /// <summary>The LedgerEvent this rule fires on.</summary>
        [Column(TypeName = "varchar(40)")]
        public string EventType { get; set; } = string.Empty;

        public int LineNo { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string DebitAccountCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string CreditAccountCode { get; set; } = string.Empty;

        /// <summary>The field on the event payload that carries this pair's amount (e.g. "Amount",
        /// "Taxable", "Tax"). A row whose field is absent or zero on a given event is skipped, so a
        /// sale with no tax simply posts no tax line.</summary>
        [Column(TypeName = "varchar(40)")]
        public string AmountField { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Narration { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
