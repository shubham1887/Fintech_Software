using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class BBPSConfiguration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BBPSConfigurationID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster? OperatorMaster { get; set; }

        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PlaceHolder { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string ErrorMeeage { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public int Length { get; set; }
        public string ConfigList { get; set; } = string.Empty;
        public bool IsDisplay { get; set; }
    }
}