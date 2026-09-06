using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Project.Model;

namespace Project.Model
{
    public class VoucherType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherTypeID { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string VoucherTypeName { get; set; }
    }
}
