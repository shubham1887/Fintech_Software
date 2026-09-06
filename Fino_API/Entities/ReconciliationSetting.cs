using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ReconciliationSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReconciliationSettingID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public virtual ProviderMaster? ProviderMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ConsumerNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Amount { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string WorksheetName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string APIUserRequestID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string SuccessStatusAs { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FailureStatusAs { get; set; }
    }
}
