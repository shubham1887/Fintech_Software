using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class MosambeeAEPSBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string BankName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string IIN { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }
    }
}