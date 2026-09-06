using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class eKYCLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int eKYCLogID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DocumentNo { get; set; }

        public int UserID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DocumentType { get; set; }
    }
}