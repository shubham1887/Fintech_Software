using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OperatorStateMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorStateMappingID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int ZoneMasterID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string HLR { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}