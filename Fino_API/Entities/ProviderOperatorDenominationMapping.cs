using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderOperatorDenominationMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderOperatorDenominationMappingID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public virtual ProviderMaster? ProviderMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        public int DenominationMasterID { get; set; }

        [ForeignKey("DenominationMasterID")]
        public virtual DenominationMaster? DenominationMaster { get; set; }
    }
}
