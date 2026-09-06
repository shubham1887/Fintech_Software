using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderDetailID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; } = null!;

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UrlType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string ProviderURL { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DateFormat { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(20)")]
        public string TransactionIDPrefix { get; set; } = string.Empty;
    }
}
