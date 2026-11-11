using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One row per commission figure that actually moved, on any slab, by anybody.
    ///
    /// Commission is the price of the product, and once slabs can be cut by resellers the question
    /// "who set this rate, and what were they allowed to set" has several possible answers and no
    /// single screen that holds them. A save that changes nothing writes nothing; a save that
    /// changes three tiers on one band writes three rows, each naming the tier, so the report reads
    /// as a list of decisions rather than a list of form submissions.
    ///
    /// Append-only by construction: nothing in the codebase updates or deletes a row here, and the
    /// repository exposes no Update or Delete at all.
    /// </summary>
    public class SlabCommissionAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SlabCommissionAuditID { get; set; }

        public int SlabMasterID { get; set; }

        /// <summary>0 for a slab-level act (created, assigned, invalidated) with no one band.</summary>
        public int SlabDetailID { get; set; }

        /// <summary>Who owns the slab that changed — 0 / null when it is the operator's own.</summary>
        public int? OwnerUserID { get; set; }

        /// <summary>The owner's upline, as it stood when the change was made.</summary>
        public int? ParentUserID { get; set; }

        /// <summary>The signed-in user who made the change. Never inferred from the owner.</summary>
        public int UpdatedByUserID { get; set; }

        public int ServiceID { get; set; }

        public int OperatorID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        /// <summary>
        /// Which figure moved: Retailer / Distributor / SuperDistributor / Client, or one of the
        /// slab-level actions (CREATE / ASSIGN / UNASSIGN / INVALIDATE / REVALIDATE).
        /// </summary>
        [Column(TypeName = "varchar(40)")]
        public string Tier { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal OldCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal NewCommission { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string OldCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string NewCommType { get; set; } = string.Empty;

        /// <summary>
        /// The ceiling the new figure was measured against at save time. Kept because the parent
        /// card can be re-cut afterwards, and "₹11 was within limit when it was set" is the whole
        /// answer to a later dispute.
        /// </summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal ParentCommission { get; set; }

        /// <summary>Free text for the slab-level actions — the slab name, or why it went invalid.</summary>
        [Column(TypeName = "varchar(400)")]
        public string Remark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(64)")]
        public string IPAddress { get; set; } = string.Empty;

        public DateTime DateTime { get; set; }
    }

    /// <summary>The <see cref="SlabCommissionAudit.Tier"/> vocabulary, in one place.</summary>
    public static class SlabAuditTier
    {
        public const string Retailer = "Retailer";
        public const string Distributor = "Distributor";
        public const string SuperDistributor = "SuperDistributor";
        public const string Client = "Client";

        public const string Create = "CREATE";
        public const string Assign = "ASSIGN";
        public const string Unassign = "UNASSIGN";
        public const string Delete = "DELETE";
        public const string Invalidate = "INVALIDATE";
        public const string Revalidate = "REVALIDATE";
        public const string GstType = "GSTType";
        public const string GstRate = "GSTRatePercent";
    }
}
