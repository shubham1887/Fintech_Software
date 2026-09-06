using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PayoutVendorBeneficiaryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutVendorBeneficiaryMappingID { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PayoutVendorBeneficiaryID
        {
            get; set;
        }

        public int PayoutBeneficaryID { get; set; }

        [ForeignKey("PayoutBeneficaryID")]
        public PayoutBeneficiary PayoutBeneficiary { get; set; }
    }
}
