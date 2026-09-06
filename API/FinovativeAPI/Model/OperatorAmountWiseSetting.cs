using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OperatorAmountWiseSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorAmountWiseSettingID { get; set; }

        public int OperatorID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        public int SerialCount { get; set; }
    }
}