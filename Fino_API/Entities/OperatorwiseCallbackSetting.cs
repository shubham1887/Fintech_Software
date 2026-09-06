using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorwiseCallbackSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorwiseCallbackSettingID { get; set; }

        public int OperatorID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Url { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string HeaderJson { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string DateFormat { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
