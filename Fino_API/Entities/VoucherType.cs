using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class VoucherType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherTypeID { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? VoucherTypeName { get; set; }
    }
}
