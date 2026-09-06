using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class RichTextMedia : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        private string _Key;

        [Column(TypeName = "varchar(max)")]
        public string Key
        {
            get => _Key;
            set => _Key = value;
        }

        private string _Content;

        [Column(TypeName = "varchar(max)")]
        public string Content
        {
            get => _Content;
            set => _Content = value;
        }
    }
}