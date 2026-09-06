using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class VendorBeneficiaryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VendorBeneficiaryMappingID { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string VendorBeneficiaryID
        {
            get; set;
        }

        public int BeneficiaryID { get; set; }

        [ForeignKey("BeneficiaryID")]
        public Beneficiary Beneficiary { get; set; }

    }
}
