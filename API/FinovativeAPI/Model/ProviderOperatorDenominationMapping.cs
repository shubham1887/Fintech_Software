using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderOperatorDenominationMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderOperatorDenominationMappingID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int DenominationMasterID { get; set; }

        [ForeignKey("DenominationMasterID")]
        public DenominationMaster DenominationMaster { get; set; }
    }
}