using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UPIVendorBeneficiaryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UPIVendorBeneficiaryMappingID { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string UPIVendorBeneficiaryID
        {
            get; set;
        }

        public int UpiBeneficiaryID { get; set; }

        [ForeignKey("UpiBeneficiaryID")]
        public UpiBeneficiary UpiBeneficiary { get; set; }

    }
}
