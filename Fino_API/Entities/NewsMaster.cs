using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class NewsMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NewsID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(1000)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; } = string.Empty;

        public bool IsView { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string ImagePath { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(200)")]
        public string HyperLink { get; set; } = string.Empty;

        /// <summary>
        /// When this announcement stops being shown. Null = it never expires.
        ///
        /// An offer, a maintenance window or a "rates revised from Monday" notice is only true
        /// for a while, but nothing took one down — the only way to stop showing a stale banner
        /// was for somebody to remember to untick Published. Past this moment the item is hidden
        /// from the network (the admin's own list still shows it, marked expired, so it can be
        /// re-dated or deleted).
        /// </summary>
        public DateTime? ExpiresOn { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
    }
}
