using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class DateFormatMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DateFormatMasterkID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string DateFormat { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
