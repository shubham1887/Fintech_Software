using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class KycDocumentMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KycDocumentMasterID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Label { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? DocumentType { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? Remark { get; set; }

        public bool IsRequired { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? RegularRxpression { get; set; }

        public string? SampleImageURL { get; set; }
    }
}
