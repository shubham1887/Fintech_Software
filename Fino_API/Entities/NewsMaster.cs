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

        public virtual UserMaster? UserMaster { get; set; }
    }
}
