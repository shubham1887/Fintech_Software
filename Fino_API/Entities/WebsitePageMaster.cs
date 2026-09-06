using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// A public website page (Home, About, Contact, ...) built by the admin in the
    /// Website Builder. Sections (hero slider, features, HTML block, ...) are stored
    /// as a JSON document so the layout is fully dynamic without schema changes.
    /// </summary>
    public class WebsitePageMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WebsitePageMasterID { get; set; }

        /// <summary>URL slug, e.g. "home", "about", "contact".</summary>
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(200)")]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(500)")]
        public string MetaDescription { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of section objects (type + fields), rendered top-to-bottom.
        /// nvarchar: page content is Unicode (₹, Hindi copy, emojis).
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string SectionsJson { get; set; } = string.Empty;

        /// <summary>Order in the public navbar (ascending).</summary>
        public int MenuOrder { get; set; }

        /// <summary>Show a navbar link for this page.</summary>
        public bool ShowInMenu { get; set; } = true;

        /// <summary>Only published pages are served to the public site.</summary>
        public bool IsPublished { get; set; }
    }
}
