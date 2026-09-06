using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderConfigurationSample : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigurationSampleID { get; set; }

        public int OperatorID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Provider
        {
            get; set;
        }

        public int ServiceID { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string OpCode { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; }
    }
}