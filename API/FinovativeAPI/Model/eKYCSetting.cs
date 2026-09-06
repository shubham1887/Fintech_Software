using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class eKYCSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int eKYCSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Slat { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string AuthKey { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ChargeAmount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string EncryptionKey { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string MerchantID { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Mode { get; set; }
    }
}