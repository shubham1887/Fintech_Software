using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderOperatorCircleMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderOperatorCircleMasterID { get; set; }

        public int ProviderID { get; set; }

        public int OperatorID { get; set; }
        public int StateID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Opcode { get; set; }

        public bool IsActive { get; set; }
    }
}