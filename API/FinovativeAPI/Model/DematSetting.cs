using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class DematSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DematSettingID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string UserName { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Token { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string UTMSource { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string EmailID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [DefaultValue(0)]
        public int Sequence { get; set; }

        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
    }
}