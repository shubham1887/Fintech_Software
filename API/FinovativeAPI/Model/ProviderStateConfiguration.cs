using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderStateConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderStateConfigurationID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int StateID { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string StateCode { get; set; }
    }
}