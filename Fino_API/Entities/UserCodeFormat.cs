using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// Per-user-type configuration for the auto-generated User Code (stored on the user as
    /// AliasName). One row per user type, each carrying its OWN running sequence, so Retailers
    /// count RD000001, RD000002 … independently of Admins counting AD000001, AD000002 …
    ///
    /// <para>The rendered code is driven by <see cref="Pattern"/>, so the shape stays a business
    /// setting rather than a code change. Supported tokens:</para>
    /// <list type="bullet">
    ///   <item><description><c>{CODE}</c> — the configured user-type code (AD / RD / DS …)</description></item>
    ///   <item><description><c>{SEQ}</c>  — the running number, zero-padded to <see cref="SequenceLength"/></description></item>
    ///   <item><description><c>{YYYY}</c> / <c>{YY}</c> — the year</description></item>
    ///   <item><description><c>{MM}</c>   — the month</description></item>
    /// </list>
    /// Anything else in the pattern is emitted literally, which covers prefix, postfix, inline and
    /// separator styles alike: <c>{CODE}{SEQ}</c> → RD000001, <c>{CODE}{YYYY}{SEQ}</c> → RD2026000001,
    /// <c>USR-{CODE}-{YYYY}-{SEQ}</c> → USR-RD-2026-000001, <c>{SEQ}{CODE}</c> → 000001RD.
    /// </summary>
    public class UserCodeFormat : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserCodeFormatID { get; set; }

        /// <summary>The user type this format belongs to (one row per type).</summary>
        public int UserTypeID { get; set; }

        [ForeignKey(nameof(UserTypeID))]
        public UserTypeMaster? UserType { get; set; }

        /// <summary>The admin-configured code for the type — AD, RD, DS …</summary>
        [Column(TypeName = "varchar(10)")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Token pattern the code is rendered from. Defaults to <c>{CODE}{SEQ}</c>.</summary>
        [Column(TypeName = "varchar(100)")]
        public string Pattern { get; set; } = "{CODE}{SEQ}";

        /// <summary>Zero-padding width of {SEQ} (6 ⇒ 000001).</summary>
        public int SequenceLength { get; set; } = 6;

        /// <summary>First number handed out (and the number a reset falls back to).</summary>
        public int StartNumber { get; set; } = 1;

        /// <summary>Last number handed out for <see cref="SequencePeriod"/>. Never decreases within a period.</summary>
        public int LastSequence { get; set; }

        /// <summary>None (never resets) | Yearly | Monthly.</summary>
        [Column(TypeName = "varchar(10)")]
        public string ResetPolicy { get; set; } = UserCodeResetPolicy.None;

        /// <summary>
        /// The period <see cref="LastSequence"/> was counted in — "" for None, "yyyy" for Yearly,
        /// "yyyyMM" for Monthly. A period change is what restarts the count at StartNumber.
        /// </summary>
        [Column(TypeName = "varchar(10)")]
        public string SequencePeriod { get; set; } = string.Empty;

        /// <summary>Off ⇒ this type falls back to the type's own alias code with a plain sequence.</summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>How often the running sequence restarts.</summary>
    public static class UserCodeResetPolicy
    {
        public const string None = "None";
        public const string Yearly = "Yearly";
        public const string Monthly = "Monthly";

        public static bool IsValid(string value) =>
            value.Equals(None, StringComparison.OrdinalIgnoreCase)
            || value.Equals(Yearly, StringComparison.OrdinalIgnoreCase)
            || value.Equals(Monthly, StringComparison.OrdinalIgnoreCase);

        /// <summary>The period key a sequence belongs to under this policy.</summary>
        public static string PeriodKey(string policy, DateTime now) =>
            policy.Equals(Yearly, StringComparison.OrdinalIgnoreCase) ? now.ToString("yyyy")
            : policy.Equals(Monthly, StringComparison.OrdinalIgnoreCase) ? now.ToString("yyyyMM")
            : string.Empty;
    }
}
