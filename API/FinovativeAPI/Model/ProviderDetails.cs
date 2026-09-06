using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderDetailID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UrlType { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProviderURL { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }

        private string _HeaderJson;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _DateFormat;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        public bool IsDelete { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string TransactionIDPrefix { get; set; }
    }
}