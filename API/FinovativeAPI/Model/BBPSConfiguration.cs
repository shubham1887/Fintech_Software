using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BBPSConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BBPSConfigurationID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public string Label { get; set; }
        public string Type { get; set; }
        public string PlaceHolder { get; set; }
        public bool IsRequired { get; set; }
        public string ErrorMeeage { get; set; }
        public int Sequence { get; set; }
        public int Length { get; set; }

        public string ConfigList { get; set; }

        public bool IsDisplay { get; set; }
    }
}