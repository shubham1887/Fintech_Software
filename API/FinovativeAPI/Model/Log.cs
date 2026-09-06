using Project.StaticModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class Log : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Thread { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Level { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Logger { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Exception { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ClientName { get; set; } = Company.ClientName;
    }
}