using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LapuMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LapuMasterID { get; set; }

        public int LapuPartyMasterID { get; set; }

        [ForeignKey("LapuPartyMasterID")]
        public virtual LapuPartyMaster? LapuPartyMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string LapuNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string ProivderLapuID { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }
    }
}
