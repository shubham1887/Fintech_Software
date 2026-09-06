using Project.Processing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderDetailsSample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderDetailsSampleID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ProviderName { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProviderURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string UrlType { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ContentType { get; set; }

        private string _DateFormat;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        public int ServiceID { get; set; }
    }
}