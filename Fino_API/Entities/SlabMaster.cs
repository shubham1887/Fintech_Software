using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SlabMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlabMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(80)")]
        public string SlabName { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        // ── Hierarchy delegation ───────────────────────────────────────────────────────────
        // A slab used to be one thing: a rate card the platform operator wrote and handed to
        // whoever they liked. These five columns make it a card with an OWNER, so a
        // super-distributor or distributor can cut their own card out of the one they were given
        // and resell inside the margin it allows. A row with every one of them at its default is
        // an admin slab exactly as before, which is what every existing row reads as.

        /// <summary>
        /// The slab this one was cut from — the owner's own assigned card. NULL on an admin slab,
        /// which answers to nobody. Every commission on a delegated slab is validated against the
        /// matching band of this one, and if this one is later lowered the child's affected bands
        /// are marked <see cref="SlabRowStatus.Invalid"/> rather than quietly paying too much.
        /// </summary>
        public int? ParentSlabID { get; set; }

        /// <summary>
        /// Who manages this slab. NULL = the platform operator (admin). Set to the
        /// super-distributor or distributor who created it, and it is the ONLY thing that decides
        /// who may edit it — every delegation endpoint checks the caller against this, never
        /// against who happens to be assigned the slab.
        /// </summary>
        public int? OwnerUserID { get; set; }

        /// <summary>
        /// The owner's role id at the time the slab was made (UserTypeCode). Stored rather than
        /// read back off the user, because it decides WHICH tier column on the card the owner is
        /// allowed to raise for themselves, and that must not change under a slab because someone
        /// re-typed the account afterwards.
        /// </summary>
        public int OwnerUserType { get; set; }

        /// <summary>Who created the row — the owner themselves, or an admin acting for them.</summary>
        public int? CreatedByUserID { get; set; }

        /// <summary>
        /// Shortcut for "this card is somebody's child card". Derivable from ParentSlabID, and
        /// stored anyway so a list query can filter the operator's own cards from the resellers'
        /// without a self-join on every screen.
        /// </summary>
        public bool IsDelegatedSlab { get; set; }

        /// <summary>
        /// ACTIVE, or INVALID when at least one band now exceeds what the parent card allows.
        /// Rolled up from the band rows (<see cref="SlabDetail.Status"/>) so a list can show the
        /// state without reading every detail row; the bands remain the authority, because only
        /// the bands that are actually out of limit stop paying.
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = SlabRowStatus.Active;
    }

    /// <summary>
    /// The two states a delegated slab (and each of its bands) can be in. A band goes INVALID the
    /// moment the parent card it was cut from stops allowing its figures — on a parent edit, or on
    /// a re-check — and the commission engine refuses that band until it is brought back inside.
    /// </summary>
    public static class SlabRowStatus
    {
        public const string Active = "ACTIVE";
        public const string Invalid = "INVALID";

        public static string Normalize(string? value) =>
            string.Equals((value ?? string.Empty).Trim(), Invalid, StringComparison.OrdinalIgnoreCase)
                ? Invalid : Active;

        public static bool IsInvalid(string? value) => Normalize(value) == Invalid;
    }
}
