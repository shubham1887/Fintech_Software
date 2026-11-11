using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One movement of a user's OUTSTANDING (the money they owe their upline for funds taken on
    /// credit) — the statement behind the Outstanding wallet.
    ///
    /// Every row is written by <c>OutstandingService</c> in the same breath as the balance change
    /// on <see cref="CashCollectionMaster"/>, and carries the opening and closing balance as the
    /// database itself returned them. That is what makes the statement reconcilable: each row's
    /// <see cref="OutstandingClosing"/> is the next row's <see cref="OutstandingOpening"/>, so a
    /// missing or duplicated posting is visible on the page rather than only in a total.
    ///
    /// <see cref="Credit"/> / <see cref="Debit"/> are markers, not amounts — the amount is always
    /// in <see cref="OutstandingAmount"/> (positive) and these say which column of the statement
    /// it belongs in. They are the shipped shape of this table and are kept as-is so existing
    /// installs need no data migration.
    /// </summary>
    public class CashCollectionDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashCollectionDetailID { get; set; }

        /// <summary>The user whose outstanding moved — the one who owes the money.</summary>
        public int UserID { get; set; }

        /// <summary>
        /// The counterparty: who extended the credit, or who collected the cash.
        ///
        /// Outstanding is per EDGE of the network, not a global figure: admin → super-distributor
        /// and distributor → retailer are separate debts owed to separate people, and this column
        /// is what says which one a row belongs to.
        /// </summary>
        public int ParentID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingOpening { get; set; }

        /// <summary>The size of the movement, always positive. Direction is in Credit/Debit.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OutstandingClosing { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ParentRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string PayMode { get; set; } = string.Empty;

        /// <summary>"CREDIT" when this row increased the outstanding; empty otherwise.</summary>
        [Column(TypeName = "varchar(50)")]
        public string Credit { get; set; } = string.Empty;

        /// <summary>"DEBIT" when this row reduced the outstanding; empty otherwise.</summary>
        [Column(TypeName = "varchar(50)")]
        public string Debit { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string UTRNumber { get; set; } = string.Empty;

        /// <summary>
        /// What caused the movement — see <c>OutstandingLedger</c> for the values. Kept separate
        /// from Credit/Debit because "a fund request taken on credit" and "a reversal put back"
        /// both increase the outstanding but are very different things to read on a statement.
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The fund transaction this posting came from, when it came from one.
        ///
        /// This is the answer to "kab liya, kaise liya" on the statement: it ties the credit back
        /// to the exact fund request or transfer, with its mode, sub-mode, receipt and UTR. Null
        /// for a cash collection, which is not a fund movement at all.
        /// </summary>
        public int? FundTransactionID { get; set; }

        /// <summary>
        /// Which wallet the funds landed in when the credit was extended.
        ///
        /// Recorded because the outstanding is wallet-agnostic on purpose — recharge, AEPS and DMT
        /// credit all roll into the one debt — but "which wallet did I take it into" is still the
        /// first thing a retailer asks when checking a line on their statement.
        /// </summary>
        public int? WalletID { get; set; }

        /// <summary>
        /// The funded wallet's balance before and after this movement.
        ///
        /// The outstanding opening/closing above say what the DEBT did; these say what the WALLET
        /// did, which is what somebody actually checks a credit line against ("1,50,000 credit -
        /// into which wallet, and what did it go from and to?"). Null for a cash collection, which
        /// moves no wallet at all - null rather than 0.00, because 0.00 would read as an empty
        /// wallet.
        /// </summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal? WalletOpening { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal? WalletClosing { get; set; }

        public bool IsDelete { get; set; }
    }
}
