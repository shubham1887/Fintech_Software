using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OperatorwiseCallbackSetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorwiseCallbackSettingID { get; set; }

        public int OperatorID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Url { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MethodType { get; set; }

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

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
