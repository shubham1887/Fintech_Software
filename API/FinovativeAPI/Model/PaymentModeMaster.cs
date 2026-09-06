using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PaymentModeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentModeMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Type { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}