using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ResponseKeyValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; }
    }
}