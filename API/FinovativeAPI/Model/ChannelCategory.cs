using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ChannelCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChannelCategoryID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Category { get; set; }
    }
}