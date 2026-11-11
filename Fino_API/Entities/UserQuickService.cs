using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One service a user has pinned to their own "Quick Services" shortcuts.
    ///
    /// This is the user's choice, not the admin's: the admin decides what the catalogue contains
    /// and what order it is shown in (ServiceMaster.UISequence), and each user then picks the
    /// handful they actually use all day and the order they want them in. The two are separate
    /// on purpose — a retailer who only ever sells mobile recharge and DMT should not have to
    /// scroll past everything else, and should not need the admin to arrange that for them.
    ///
    /// Stored server-side rather than in browser storage so the same shortcuts follow the user
    /// from the web panel to the Android app and to a new device.
    /// </summary>
    public class UserQuickService : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserQuickServiceID { get; set; }

        public int UserID { get; set; }

        public int ServiceID { get; set; }

        /// <summary>Position among this user's shortcuts; lower shows first.</summary>
        public int UISequence { get; set; }
    }
}
