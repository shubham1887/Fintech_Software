using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class FingerPrintDeviceType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FingerPrintDeviceTypeID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string DeviceName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Value { get; set; } = string.Empty;
    }
}
