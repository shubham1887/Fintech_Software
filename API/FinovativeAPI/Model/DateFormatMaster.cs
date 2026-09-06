using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class DateFormatMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DateFormatMasterkID { get; set; }

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
    }
}