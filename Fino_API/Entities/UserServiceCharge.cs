using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserServiceCharge : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserServiceChargeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ServiceID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; }
    }
}
