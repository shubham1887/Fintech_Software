using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class BulkPaySetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BulkPaySettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Slat { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ActionURL { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string StatusCheckURL { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MinimumAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal MaximumAmount { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; }
         
        [DefaultValue(false)]
        public bool IsPendingForFailed { get; set; }
        
    }
}