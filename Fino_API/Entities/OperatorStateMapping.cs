using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorStateMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorStateMappingID { get; set; }
        public int OperatorID { get; set; }
        public int ZoneMasterID { get; set; }
        [Column(TypeName = "varchar(max)")] public string? HLR { get; set; }
        public bool IsActive { get; set; }
        public virtual OperatorMaster? OperatorMaster { get; set; }
    }
}
