using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class PayoutVendorBeneficiaryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutVendorBeneficiaryMappingID { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PayoutVendorBeneficiaryID { get; set; } = string.Empty;

        public int PayoutBeneficaryID { get; set; }

        [ForeignKey("PayoutBeneficaryID")]
        public virtual PayoutBeneficiary? PayoutBeneficiary { get; set; }
    }
}
