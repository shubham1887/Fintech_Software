using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One user's authenticator-app enrolment: the shared secret, whether they finished setting
    /// it up, and the recovery codes that get them back in when the phone is gone.
    ///
    /// Deliberately NOT columns on UserMaster. The secret is a credential — anyone holding it can
    /// generate that user's codes forever — and UserMaster is read in bulk, cached in memory for
    /// the lifetime of the process and mapped into a dozen response DTOs. A secret living there
    /// is one careless `MapToDetail` away from being serialised to a client. Here, nothing reads
    /// the row except the service that verifies against it.
    /// </summary>
    public class UserTwoFactor : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserTwoFactorID { get; set; }

        public int UserID { get; set; }

        /// <summary>The Base32 shared secret the authenticator app holds a copy of.</summary>
        [Column(TypeName = "varchar(64)")]
        public string SecretBase32 { get; set; } = string.Empty;

        /// <summary>
        /// False between "show me the QR" and "here is a code from my phone".
        ///
        /// An unconfirmed row must never gate a login: if it did, scanning a QR and then closing
        /// the page — or scanning it into an app that was pointed at the wrong account — would
        /// lock the user out of their own panel with a secret nothing can produce codes for.
        /// </summary>
        public bool IsConfirmed { get; set; }

        public DateTime? ConfirmedOn { get; set; }

        /// <summary>
        /// The last 30-second step a code was accepted for. A code stays valid for up to ninety
        /// seconds, so without this the same six digits — read over a shoulder, or left on a
        /// screen — could be used a second time. Verification refuses any step at or below it.
        /// </summary>
        public long LastUsedStep { get; set; }

        /// <summary>
        /// Hashes of the unused recovery codes, as a JSON array. Hashed, not stored, for the same
        /// reason passwords are: they are single-use passwords. Each one is removed from the list
        /// as it is spent.
        /// </summary>
        [Column(TypeName = "varchar(max)")]
        public string RecoveryCodeHashes { get; set; } = string.Empty;
    }
}
