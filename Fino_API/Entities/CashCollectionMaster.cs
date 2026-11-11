using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CashCollectionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashCollectionMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Outstanding { get; set; }

        /// <summary>
        /// This account's own credit limit. Only meaningful when <see cref="HasExplicitLimit"/> is
        /// set; otherwise the Key Setting default applies.
        /// </summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal MaxOutstanding { get; set; }

        /// <summary>
        /// Has somebody actually decided this account's limit?
        ///
        /// Without this the single MaxOutstanding column cannot tell "never configured, so use the
        /// default" from "deliberately set to zero, so no credit even though a default exists" -
        /// and the default would silently re-open credit for the one retailer an admin had just
        /// blocked. Every row is created at zero and NOT explicit; setting a limit from the screen
        /// (including setting it to zero) makes it explicit.
        /// </summary>
        public bool HasExplicitLimit { get; set; }
    }
}
