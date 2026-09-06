using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class RichTextMedia : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Key { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Content { get; set; } = string.Empty;
    }
}
