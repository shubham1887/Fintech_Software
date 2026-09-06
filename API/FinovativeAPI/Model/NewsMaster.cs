using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
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
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        public bool IsView { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string ImagePath { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string HyperLink { get; set; }
    }
}