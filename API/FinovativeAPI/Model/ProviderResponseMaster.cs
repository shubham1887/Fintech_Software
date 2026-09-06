using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderResponseMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderResponseMasterID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string TextMustexits { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Status
        {
            get; set;
        }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string Response { get; set; }

        public bool IsReference { get; set; }

        public bool IsActive { get; set; }
    }
}