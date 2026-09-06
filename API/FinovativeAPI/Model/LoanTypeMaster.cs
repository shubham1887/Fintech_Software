using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Model
{
    public class LoanTypeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanTypeMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string LoanType { get; set; }
    }
}
