using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PaymentModeMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentModeMasterID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
