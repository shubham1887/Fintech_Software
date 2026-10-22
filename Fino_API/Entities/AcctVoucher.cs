using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// A journal voucher: one balanced posting to the ledger. TotalDebit always equals TotalCredit —
    /// the posting engine refuses to write a voucher that does not balance, so an unbalanced book is
    /// impossible by construction rather than caught later.
    ///
    /// A voucher is never edited once posted. A mistake is corrected by a reversing voucher, so the
    /// trail shows both the error and the correction instead of a figure that silently changed.
    /// </summary>
    public class AcctVoucher : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctVoucherID { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string VoucherNo { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; }

        /// <summary>yyyyMM — the period the voucher posts into, checked against the period lock.</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>The event that produced it (LedgerEvent), or ManualJournal.</summary>
        [Column(TypeName = "varchar(40)")]
        public string VoucherType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(300)")]
        public string Narration { get; set; } = string.Empty;

        /// <summary>The outbox row this came from, when it was posted by the engine. 0 for a manual journal.</summary>
        public int SourceOutboxID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; }

        /// <summary>Posted / Reversed.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        /// <summary>The voucher that reverses this one, or the one this reverses. Set on both.</summary>
        public int? ReversalOfVoucherID { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>One line of a voucher: a debit XOR a credit to one account.</summary>
    public class AcctVoucherLine : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctVoucherLineID { get; set; }

        public int AcctVoucherID { get; set; }

        [ForeignKey(nameof(AcctVoucherID))]
        public virtual AcctVoucher? Voucher { get; set; }

        public int AcctAccountID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string AccountCode { get; set; } = string.Empty;

        public int LineNo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Narration { get; set; } = string.Empty;
    }
}
