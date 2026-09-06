using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class LapuMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LapuMasterID { get; set; }

        public int LapuPartyMasterID { get; set; }

        [ForeignKey("LapuPartyMasterID")]
        public LapuPartyMaster LapuPartyMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string LapuNo { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ProivderLapuID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }
    }
}