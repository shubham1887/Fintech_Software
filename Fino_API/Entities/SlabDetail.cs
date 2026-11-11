using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SlabDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlabDetailID { get; set; }

        public int SlabMasterID { get; set; }

        [ForeignKey("SlabMasterID")]
        public SlabMaster SlabMaster { get; set; } = null!;

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } = null!;

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RetailerCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RetailerCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string SuperDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string SuperDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClientComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ClientCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ClientCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealRetailerComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealRetailerCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealRetailerCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealSuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealSuperDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealSuperDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealClientComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealClientCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealClientCommissionCategory { get; set; } = string.Empty;


        /// <summary>
        /// Whether the charges on THIS band already contain GST — INCLUSIVE / EXCLUSIVE / NONE —
        /// or DEFAULT (the shipped value) to fall through to the commission tax profile.
        ///
        /// It applies to every tier on the row, because a slab band is one commercial decision:
        /// the retailer's ₹8, and whatever the row pays the uplines out of it, are taxed the same
        /// way. Per-band is the level that matters — a card can read "500-1000 inclusive,
        /// 1001-5000 exclusive, 5001-25000 no GST", which no per-slab setting can express.
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string GSTType { get; set; } = StaticModels.ChargeGstType.Default;

        /// <summary>The rate for this band. 0 = ask the tax engine, effective-dated.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal GSTRatePercent { get; set; }

        public bool IsActive { get; set; } = true;

        // ── Hierarchy delegation: what the parent card allowed when this band was saved ──────
        // These carry no weight in the commission calculation — the engine still reads the tier
        // columns above and nothing else. They exist so a validation refusal can name the figure
        // it was measured against, and so an audit six months later can say what the ceiling WAS
        // at the moment the owner set their rate, rather than what it happens to be today.
        // All default to NULL / 0, which is what every band on an admin slab reads as.

        /// <summary>The band on the parent card this one was cut from. NULL on an admin slab.</summary>
        public int? ParentSlabDetailID { get; set; }

        /// <summary>The parent band's retailer figure — the ceiling Rule 1 measured against.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal ParentRetailerComm { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ParentDistributorComm { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ParentSuperDistributorComm { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ParentClientComm { get; set; }

        /// <summary>
        /// The parent band's commission type (Flexi / Fix). A child inherits it and cannot change
        /// it — a percentage ceiling means nothing once the child switches the row to a flat
        /// amount, so Rule 3 pins the two together.
        /// </summary>
        [Column(TypeName = "varchar(100)")]
        public string ParentCommType { get; set; } = string.Empty;

        /// <summary>The parent band's GST decision, which the child inherits whole (Rule 5).</summary>
        [Column(TypeName = "varchar(20)")]
        public string ParentGSTType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal ParentGSTRatePercent { get; set; }

        /// <summary>
        /// ACTIVE, or INVALID when the parent band was later lowered past what this one pays out.
        /// The commission engine refuses an INVALID band outright instead of falling through to
        /// the parent's card, because falling through would pay MORE than the owner now allows —
        /// the exact mistake invalidation exists to prevent. Only the bands actually out of limit
        /// go invalid, so one corrected rate on one operator does not stop a whole network
        /// trading.
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = SlabRowStatus.Active;
    }
}
