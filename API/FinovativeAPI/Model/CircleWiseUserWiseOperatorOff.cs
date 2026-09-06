using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class CircleWiseUserWiseOperatorOff : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleWiseUserWiseOperatorOffID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int StateID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }
    }
}