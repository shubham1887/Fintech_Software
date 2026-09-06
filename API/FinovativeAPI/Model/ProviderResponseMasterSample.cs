using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderResponseMasterSample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderResponseMasterSampleID { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Provider
        {
            get; set;
        }

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
    }
}